const express = require('express');
const pool = require('../db');

const router = express.Router();

router.get('/', async (req, res, next) => {
  try {
    const status = String(req.query.status || 'todos').toLowerCase();
    const search = String(req.query.search || '');
    const filters = [];
    const values = [];

    if (status === 'disponivel') filters.push('available_quantity > 0');
    if (status === 'emprestado') filters.push('available_quantity < total_quantity');
    if (search.trim()) {
      filters.push('(name LIKE ? OR code LIKE ?)');
      const term = `%${search.trim()}%`;
      values.push(term, term);
    }

    const where = filters.length ? `WHERE ${filters.join(' AND ')}` : '';
    const [rows] = await pool.query(
      `SELECT id, code, name, description, total_quantity, available_quantity,
              total_quantity - available_quantity AS loaned_quantity,
              created_at
       FROM tools ${where}
       ORDER BY name`,
      values,
    );

    res.json(rows);
  } catch (error) {
    next(error);
  }
});

router.post('/', async (req, res, next) => {
  try {
    const body = req.body && typeof req.body === 'object' ? req.body : {};
    const code = String(body.code || '').trim().toUpperCase();
    const name = String(body.name || '').trim();
    const description = String(body.description || '').trim();
    const quantity = Number(body.quantity);

    if (!code || !name || !Number.isInteger(quantity) || quantity < 1 ||
        quantity > 4294967295 || code.length > 50 || name.length > 120 ||
        description.length > 255) {
      return res.status(400).json({
        message: 'Revise código, nome, descrição e quantidade informados.',
      });
    }

    const [result] = await pool.execute(
      `INSERT INTO tools
         (code, name, description, total_quantity, available_quantity)
       VALUES (?, ?, ?, ?, ?)`,
      [code, name, description || null, quantity, quantity],
    );

    const [[tool]] = await pool.execute('SELECT * FROM tools WHERE id = ?', [result.insertId]);
    res.status(201).json(tool);
  } catch (error) {
    if (error.code === 'ER_DUP_ENTRY') {
      return res.status(409).json({ message: 'Já existe uma ferramenta com esse código.' });
    }
    next(error);
  }
});

module.exports = router;
