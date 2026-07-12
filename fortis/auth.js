(function(){
  'use strict';

  var destinos = {
    admin: 'painel-admin.html',
    aluno: 'painel-usuario.html',
    prof: 'painel-professor.html'
  };

  var form = document.getElementById('loginForm');
  var usuario = document.getElementById('loginUsuario');
  var erro = document.getElementById('loginError');

  if (!form || !usuario || !erro) return;

  form.addEventListener('submit', function(event){
    event.preventDefault();

    var perfil = usuario.value.trim().toLowerCase();
    var destino = destinos[perfil];

    if (!destino) {
      erro.textContent = 'Usuário inválido. Digite admin, aluno ou prof.';
      usuario.setAttribute('aria-invalid', 'true');
      usuario.focus();
      return;
    }

    erro.textContent = '';
    usuario.removeAttribute('aria-invalid');
    window.location.href = destino;
  });

  usuario.addEventListener('input', function(){
    erro.textContent = '';
    usuario.removeAttribute('aria-invalid');
  });
})();
