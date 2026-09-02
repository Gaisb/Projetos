require('dotenv').config();

const path = require('path');
const cors = require('cors');
const express = require('express');
const pool = require('./db');
const toolsRouter = require('./routes/tools');
const loansRouter = require('./routes/loans');

const app = express();
const port = Number(process.env.PORT || 3000);

app.use(cors());
app.use(express.json());
app.use(express.static(path.join(__dirname, '..', 'public')));

app.get('/api/health', async (_req, res, next) => {
  try {
    await pool.query('SELECT 1');
    res.json({ status: 'ok', database: 'connected' });
  } catch (error) {
    next(error);
  }
});

app.use('/api/tools', toolsRouter);
app.use('/api/loans', loansRouter);

app.use('/api', (_req, res) => {
  res.status(404).json({ message: 'Rota não encontrada.' });
});

app.use((error, _req, res, _next) => {
  if (error.type === 'entity.parse.failed') {
    return res.status(400).json({ message: 'O corpo da requisição deve conter um JSON válido.' });
  }
  console.error(error);
  res.status(500).json({ message: 'Erro interno do servidor.' });
});

app.listen(port, () => {
  console.log(`ControleLab disponível em http://localhost:${port}`);
});
