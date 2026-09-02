const express = require('express');
const pool = require('../db');

const router = express.Router();

router.get('/', async (req, res, next) => {
  try {
    const status = String(req.query.status || 'EMPRESTADO');
    const allowedStatuses = ['EMPRESTADO', 'DEVOLVIDO', 'TODOS'];
    const safeStatus = allowedStatuses.includes(status.toUpperCase())
      ? status.toUpperCase()
      : 'EMPRESTADO';
    const where = safeStatus === 'TODOS' ? '' : 'WHERE l.status = ?';
    const values = safeStatus === 'TODOS' ? [] : [safeStatus];

    const [rows] = await pool.query(
      `SELECT l.id, l.student_name, l.student_registration, l.responsible_name,
              l.quantity, l.loaned_at, l.returned_at, l.return_responsible_name,
              l.status, l.notes, t.id AS tool_id, t.code AS tool_code,
              t.name AS tool_name
       FROM loans l
       INNER JOIN tools t ON t.id = l.tool_id
       ${where}
       ORDER BY l.loaned_at DESC`,
      values,
    );

    res.json(rows);
  } catch (error) {
    next(error);
  }
});

router.post('/', async (req, res, next) => {
  let connection;
  try {
    connection = await pool.getConnection();
    const body = req.body && typeof req.body === 'object' ? req.body : {};
    const toolId = Number(body.toolId);
    const quantity = Number(body.quantity);
    const studentName = String(body.studentName || '').trim();
    const studentRegistration = String(body.studentRegistration || '').trim();
    const responsibleName = String(body.responsibleName || '').trim();
    const notes = String(body.notes || '').trim();

    if (!Number.isInteger(toolId) || !studentName || !responsibleName ||
        !Number.isInteger(quantity) || quantity < 1 || quantity > 4294967295 ||
        studentName.length > 120 || studentRegistration.length > 40 ||
        responsibleName.length > 120 || notes.length > 255) {
      return res.status(400).json({
        message: 'Ferramenta, aluno, responsável e quantidade válida são obrigatórios.',
      });
    }

    await connection.beginTransaction();
    const [[tool]] = await connection.execute(
      'SELECT id, available_quantity FROM tools WHERE id = ? FOR UPDATE',
      [toolId],
    );

    if (!tool) {
      await connection.rollback();
      return res.status(404).json({ message: 'Ferramenta não encontrada.' });
    }
    if (tool.available_quantity < quantity) {
      await connection.rollback();
      return res.status(409).json({ message: 'Quantidade indisponível para empréstimo.' });
    }

    const [result] = await connection.execute(
      `INSERT INTO loans
         (tool_id, student_name, student_registration, responsible_name, quantity, notes)
       VALUES (?, ?, ?, ?, ?, ?)`,
      [toolId, studentName, studentRegistration || null, responsibleName, quantity, notes || null],
    );
    await connection.execute(
      'UPDATE tools SET available_quantity = available_quantity - ? WHERE id = ?',
      [quantity, toolId],
    );
    await connection.commit();

    res.status(201).json({ id: result.insertId, message: 'Empréstimo registrado.' });
  } catch (error) {
    if (connection) await connection.rollback();
    next(error);
  } finally {
    if (connection) connection.release();
  }
});

router.patch('/:id/return', async (req, res, next) => {
  let connection;
  try {
    connection = await pool.getConnection();
    const body = req.body && typeof req.body === 'object' ? req.body : {};
    const loanId = Number(req.params.id);
    const responsibleName = String(body.responsibleName || '').trim();
    if (!Number.isInteger(loanId) || !responsibleName || responsibleName.length > 120) {
      return res.status(400).json({ message: 'Informe o responsável pela devolução.' });
    }

    await connection.beginTransaction();
    const [[loan]] = await connection.execute(
      'SELECT tool_id, quantity, status FROM loans WHERE id = ? FOR UPDATE',
      [loanId],
    );

    if (!loan) {
      await connection.rollback();
      return res.status(404).json({ message: 'Empréstimo não encontrado.' });
    }
    if (loan.status === 'DEVOLVIDO') {
      await connection.rollback();
      return res.status(409).json({ message: 'Este empréstimo já foi devolvido.' });
    }

    await connection.execute(
      `UPDATE loans
       SET status = 'DEVOLVIDO', returned_at = CURRENT_TIMESTAMP,
           return_responsible_name = ?
       WHERE id = ?`,
      [responsibleName, loanId],
    );
    await connection.execute(
      'UPDATE tools SET available_quantity = available_quantity + ? WHERE id = ?',
      [loan.quantity, loan.tool_id],
    );
    await connection.commit();

    res.json({ message: 'Devolução registrada com sucesso.' });
  } catch (error) {
    if (connection) await connection.rollback();
    next(error);
  } finally {
    if (connection) connection.release();
  }
});

module.exports = router;
