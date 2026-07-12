# FORTIS

*O app para personal trainers.*

**Menos planilha. Mais resultado.**

---

## Sobre

FORTIS é uma proposta de aplicativo pensado para o personal trainer que vive entre WhatsApp, Excel e papel. O objetivo é reunir, em um só lugar, tudo o que hoje está espalhado: anamnese, avaliação física, prescrição de treino e cobrança.

Este repositório apresenta o **conceito visual inicial** do projeto — não um produto final, mas uma direção de design e arquitetura pronta para ser validada e evoluída.

## Páginas

O projeto é composto por cinco páginas, todas construídas com HTML, CSS e JavaScript puros, sem dependências de build.

- **Início** (`index.html`) — Apresentação do conceito: o problema, a proposta de módulos, o fluxo do aluno e um esboço de tela.
- **Calculadora de TMB** (`calculo-basal.html`) — Ferramenta funcional que calcula a Taxa Metabólica Basal pela fórmula de Mifflin-St Jeor e estima o gasto calórico diário.
- **Agente IA** (`agente-ia.html`) — Prévia visual de um chat de assistência. As respostas são exemplos fixos; nenhuma inteligência artificial real está conectada.
- **Contato** (`contato.html`) — Formulário que monta um `mailto:` e um atalho direto para WhatsApp.
- **Login / Cadastro** (`login.html`) — Painel duplo animado que alterna entre entrar e criar conta.
- **Tela de usuário e professor** ('aluno/professor.html') - Painel funcional com apresentação de widgets.

## Detalhes técnicos

### Transições entre páginas

A navegação entre páginas conta com um fade suave ao sair e ao entrar, controlado por um único script compartilhado (`transitions.js`). O efeito respeita:

- âncoras da própria página (mantém o scroll suave);
- abertura em nova aba, `mailto:` e `tel:`;
- links externos;
- a preferência `prefers-reduced-motion` (acessibilidade).

### Painel duplo de autenticação

A tela de login/cadastro utiliza uma arquitetura de "cortina deslizante": o formulário ativo permanece em uma faixa fixa, enquanto um painel colorido desliza de um lado para o outro, revelando o formulário correto. Em telas pequenas, o layout passa a usar abas.

### Paleta e tipografia

- **Cores:** fundo escuro (`#11140F`), osso (`#F3EFE3`), brasa laranja (`#FF5A2E`) e dourado (`#C9A227`).
- **Tipografia:** *Big Shoulders Display* (títulos), *Space Grotesk* (texto) e *IBM Plex Mono* (detalhes).

## Como visualizar

Por ser um site estático, basta abrir qualquer arquivo `.html` diretamente no navegador. Não há servidor, instalação ou build.

```
1. Baixe ou clone o repositório
2. Abra index.html no navegador
3. Navegue entre as páginas
```

## Estado do projeto

Esta é a **versão 0.5 — proposta inicial**. Trata-se de um material de apresentação, sem vínculo com terceiros, e sem conexão com qualquer backend. Os formulários e o chat são apenas ilustrativos.

## Próximos passos

- Definir nome, cores e tipografia finais;
- Evoluir para as telas detalhadas de cadastro, treino e avaliação;
- Conectar a um backend real e a um modelo de IA.

---

*Conceito de apresentação · sem vínculo com MFIT Personal ou terceiros.

https://gaisb.github.io/FitApp/*


