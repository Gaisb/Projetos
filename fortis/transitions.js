/* Bootstrap 5 (bundle inclui Popper) disponível em todas as páginas. */
(function(){
  'use strict';

  if (window.bootstrap) return;

  var script = document.createElement('script');
  script.src = 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js';
  script.integrity = 'sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz';
  script.crossOrigin = 'anonymous';
  document.head.appendChild(script);
})();

/* Remove do fluxo os links do antigo seletor de painéis. */
document.querySelectorAll('a[href="selecao-painel.html"]').forEach(function(link){
  link.href = 'login.html';
  link.lastChild.textContent = ' Sair';
});

/* ===========================================================
   TRANSIÇÃO FLUIDA ENTRE PÁGINAS
   -----------------------------------------------------------
   Faz um fade suave ao SAIR de uma página antes de navegar,
   e um fade-in ao ENTRAR (controlado via CSS no style.css,
   regras body / body.fortis-leaving).

   Respeita prefers-reduced-motion e NÃO interfere em:
     - âncoras da própria página (#recursos, #funciona...)
     - abas novas (target="_blank") e downloads
     - atalhos com modificadores (Ctrl/Cmd/Shift/Alt)
     - mailto:, tel: e javascript:
     - links externos (origem diferente)
   =========================================================== */
(function(){
  'use strict';

  var prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  var DURACAO_FADE = 300; // precisa bater com o .fortis-leaving no style.css

  document.addEventListener('click', function(e){
    // só botão principal do mouse
    if(e.button !== 0) return;

    var link = e.target.closest && e.target.closest('a');
    if(!link) return;

    // deixa o navegador cuidar: nova aba, download, atalhos de teclado
    if(e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
    if(link.target === '_blank') return;
    if(link.hasAttribute('download')) return;

    var href = link.getAttribute('href');
    if(!href) return;

    // âncoras da própria página e protocolos especiais
    if(href.charAt(0) === '#') return;
    if(href.indexOf('mailto:') === 0) return;
    if(href.indexOf('tel:') === 0) return;
    if(href.indexOf('javascript:') === 0) return;

    var url;
    try{
      url = new URL(link.href, window.location.href);
    }catch(_){
      return; // href inválido — deixa o navegador tratar
    }

    // origem diferente = link externo
    if(url.origin !== window.location.origin) return;

    // exatamente a mesma URL — nada a fazer
    if(url.href === window.location.href) return;

    // mesma página com hash (#algo) — deixa o scroll suave agir
    if(url.pathname === window.location.pathname && url.hash) return;

    e.preventDefault();

    if(prefersReduced){
      window.location.href = link.href;
      return;
    }

    document.body.classList.add('fortis-leaving');
    window.setTimeout(function(){
      window.location.href = link.href;
    }, DURACAO_FADE);
  });

  // se a página voltar via bfcache no meio de um fade, reseta o estado
  window.addEventListener('pageshow', function(e){
    if(e.persisted){
      document.body.classList.remove('fortis-leaving');
    }
  });
})();
