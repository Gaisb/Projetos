# ControleLab

MVP de um sistema web para controlar empréstimos e devoluções de ferramentas e equipamentos em laboratórios escolares.

## O que já funciona

- cadastro de ferramentas com código único, nome, descrição e quantidade;
- consulta e filtro por disponibilidade;
- empréstimo vinculado ao aluno e ao responsável;
- bloqueio quando não há quantidade disponível;
- devolução com responsável, data e hora;
- painel com indicadores;
- relatório/histórico simples de movimentações;
- banco MySQL com dados iniciais para demonstração.

## Tecnologias

- Front-end: HTML, Tailwind CSS (CDN), CSS e JavaScript;
- Back-end: Node.js e Express;
- Banco de dados: MySQL;
- Comunicação: API REST em JSON.

## Como executar

1. Instale Node.js 18 ou superior e MySQL 8.
2. No MySQL Workbench, abra e execute `database/schema.sql`.
3. Copie `.env.example` para `.env` e informe a senha do MySQL.
4. Abra o terminal na pasta do projeto e execute:

```bash
npm install
npm start
```

5. Acesse `http://localhost:3000`.

## Demonstração recomendada

1. Mostre o painel e os indicadores.
2. Cadastre uma ferramenta com código novo.
3. Registre um empréstimo para um aluno.
4. Mostre a redução da quantidade disponível.
5. Tente emprestar além do estoque para demonstrar a regra de negócio.
6. Registre a devolução.
7. Abra o histórico e mostre data, hora e status.

## Próximas melhorias

- login e níveis de acesso;
- cadastro próprio de alunos e responsáveis;
- data prevista e aviso de atraso;
- edição e inativação de ferramentas;
- exportação do relatório em PDF/CSV;
- testes automatizados e publicação em servidor.
