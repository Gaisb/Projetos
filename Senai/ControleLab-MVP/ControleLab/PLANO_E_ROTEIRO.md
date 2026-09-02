# Plano de desenvolvimento e apresentação do ControleLab

## 1. Entender e delimitar o problema

O processo atual usa planilha ou caderno e dificulta descobrir quem está com cada ferramenta. O MVP deve resolver somente o fluxo essencial: cadastrar, emprestar, consultar e devolver.

## 2. Definir usuários e objetivo

- Usuários: responsável pelo laboratório/oficina e alunos atendidos.
- Objetivo: registrar digitalmente as movimentações e reduzir perdas e esquecimentos.

## 3. Organizar as funcionalidades

1. Cadastro de ferramenta/equipamento.
2. Listagem e busca por código, nome ou status.
3. Registro de empréstimo vinculado ao aluno.
4. Registro de devolução.
5. Histórico/relatório de itens emprestados.

## 4. Aplicar as regras do PDF

- código da ferramenta não pode se repetir;
- não pode emprestar uma quantidade indisponível;
- empréstimo e devolução registram responsável, data e hora;
- campos essenciais precisam ser validados;
- os dados ficam em banco relacional.

## 5. Modelar o banco

- `tools`: dados e quantidades das ferramentas;
- `loans`: aluno, ferramenta, quantidade, responsáveis, datas e status;
- relacionamento: uma ferramenta pode aparecer em vários empréstimos ao longo do tempo.

## 6. Construir a API

- `GET /api/tools`: listar e filtrar ferramentas;
- `POST /api/tools`: cadastrar ferramenta;
- `GET /api/loans`: consultar movimentações;
- `POST /api/loans`: registrar empréstimo;
- `PATCH /api/loans/:id/return`: registrar devolução.

## 7. Construir as telas

- painel com indicadores;
- tabela de ferramentas;
- formulário de cadastro;
- formulário de empréstimo;
- tela de empréstimos ativos;
- histórico de movimentações.

## 8. Testar antes da apresentação

- cadastrar uma ferramenta válida;
- tentar repetir o código;
- emprestar uma unidade disponível;
- tentar emprestar mais do que o estoque;
- devolver e conferir o estoque;
- revisar o histórico.

## 9. Roteiro sugerido para os slides

1. Capa: ControleLab, equipe e turma.
2. Problema atual: planilhas/caderno, perdas e falta de rastreabilidade.
3. Público-alvo e objetivo do MVP.
4. Funcionalidades essenciais.
5. Requisitos e regras de negócio.
6. Tecnologias e arquitetura: navegador, API Node.js e MySQL.
7. Modelo de dados: ferramentas e empréstimos.
8. Telas do sistema e demonstração.
9. Testes realizados e resultados.
10. Próximos passos e conclusão.

## 10. Fala curta para apresentar

“O ControleLab surgiu para substituir o controle manual de ferramentas feito em planilhas ou cadernos. O MVP permite cadastrar itens, registrar empréstimos vinculados aos alunos, controlar devoluções e visualizar o histórico. Usamos HTML e Tailwind no front-end, Node.js no back-end e MySQL para persistir os dados. As principais regras impedem códigos duplicados e empréstimos sem estoque, além de registrar o responsável e o horário de cada operação. Assim, a escola ganha rastreabilidade, agilidade e reduz o risco de perda de equipamentos.”
