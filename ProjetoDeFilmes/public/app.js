const $ = (selector) => document.querySelector(selector);
const state = { q: '', genero: '', ordem: 'nota_desc', pagina: 1 };
let controller;

function escapeHtml(value) {
  const el = document.createElement('span');
  el.textContent = value ?? '';
  return el.innerHTML;
}

function syncUrl() {
  const entries = Object.entries(state).filter(
    ([, value]) => value && value !== 1 && value !== 'nota_desc',
  );
  const params = new URLSearchParams(entries);
  history.replaceState(null, '', params.size ? `?${params}` : '/');
}

function skeletons() {
  $('#movies').innerHTML = Array.from(
    { length: 8 },
    () => '<article class="card skeleton"><div></div><i></i><i></i></article>',
  ).join('');
}

async function loadMovies({ scroll = false } = {}) {
  controller?.abort();
  controller = new AbortController();
  skeletons();
  $('#empty').hidden = true;
  $('#movies').setAttribute('aria-busy', 'true');
  syncUrl();

  try {
    const params = new URLSearchParams(state);
    const response = await fetch(`/api/filmes?${params}`, {
      signal: controller.signal,
    });

    if (!response.ok) {
      throw new Error('Não foi possível carregar o catálogo.');
    }

    const { dados, paginacao } = await response.json();
    renderMovies(dados);
    renderPagination(paginacao);

    const label = paginacao.total === 1 ? '1 filme encontrado' : `${paginacao.total} filmes encontrados`;
    $('#resultCount').textContent = label;
    $('#empty').hidden = paginacao.total !== 0;
    $('#movies').hidden = paginacao.total === 0;
    $('#activeSearch').hidden = !state.q;
    $('#activeSearch').innerHTML = state.q
      ? `Resultados para <strong>“${escapeHtml(state.q)}”</strong> <button aria-label="Remover busca">×</button>`
      : '';
    $('#activeSearch button')?.addEventListener('click', clearSearch);

    if (scroll) {
      $('.catalog').scrollIntoView({ behavior: 'smooth' });
    }
  } catch (error) {
    if (error.name !== 'AbortError') {
      $('#movies').innerHTML = `<div class="error">${escapeHtml(error.message)} <button onclick="loadMovies()">Tentar novamente</button></div>`;
    }
  } finally {
    $('#movies').setAttribute('aria-busy', 'false');
  }
}

function renderMovies(movies) {
  $('#movies').innerHTML = movies.map(
    (movie, index) => `<article class="card" style="--delay:${index * 35}ms">
    <div class="poster tone-${(movie.id % 6) + 1}">
      <span class="number">${String(movie.id).padStart(2, '0')}</span>
      <span class="reel">◉</span>
      <p>${escapeHtml(movie.genero)}</p>
    </div>
    <div class="card-body">
      <div class="meta">
        <span>${movie.ano_lancamento}</span>
        <span>${movie.duracao_minutos} min</span>
        <span class="rating">★ ${movie.nota_imdb.toFixed(1)}</span>
      </div>
      <h3>${escapeHtml(movie.titulo)}</h3>
      <p class="director">Direção de ${escapeHtml(movie.diretor)}</p>
    </div>
  </article>`,
  ).join('');
}

function renderPagination(p) {
  if (p.paginas <= 1) {
    $('#pagination').innerHTML = '';
    return;
  }

  const pages = [
    ...new Set(
      [1, p.pagina - 1, p.pagina, p.pagina + 1, p.paginas].filter(
        (number) => number > 0 && number <= p.paginas,
      ),
    ),
  ];
  let last = 0;
  const buttons = [];

  for (const number of pages) {
    if (number - last > 1) {
      buttons.push('<span>…</span>');
    }
    buttons.push(`<button data-page="${number}" ${number === p.pagina ? 'aria-current="page"' : ''}>${number}</button>`);
    last = number;
  }

  $('#pagination').innerHTML = `<button data-page="${p.pagina - 1}" ${p.pagina === 1 ? 'disabled' : ''}>←</button>${buttons.join('')}<button data-page="${p.pagina + 1}" ${p.pagina === p.paginas ? 'disabled' : ''}>→</button>`;
  $('#pagination').querySelectorAll('[data-page]').forEach((button) => {
    button.addEventListener('click', () => {
      state.pagina = +button.dataset.page;
      loadMovies({ scroll: true });
    });
  });
}

function clearSearch() {
  state.q = '';
  state.genero = '';
  state.pagina = 1;
  $('#query').value = '';
  $('#genre').value = '';
  loadMovies();
}

async function init() {
  const params = new URLSearchParams(location.search);

  for (const key of Object.keys(state)) {
    if (params.has(key)) {
      state[key] = key === 'pagina' ? +params.get(key) : params.get(key);
    }
  }

  $('#query').value = state.q;
  $('#sort').value = state.ordem;

  try {
    const data = await fetch('/api/filtros').then((response) => response.json());
    const genreOptions = data.generos
      .map((genre) => `<option>${escapeHtml(genre)}</option>`)
      .join('');
    $('#genre').insertAdjacentHTML('beforeend', genreOptions);
    $('#genre').value = state.genero;

    const stats = data.estatisticas;
    $('#stats').innerHTML = `
      <div><strong>${stats.total}</strong><span>filmes selecionados</span></div>
      <div><strong>${stats.media}</strong><span>nota média IMDb</span></div>
      <div><strong>${stats.primeiro_ano}—${stats.ultimo_ano}</strong><span>décadas de histórias</span></div>
    `;
  } catch {
    $('#stats').hidden = true;
  }

  loadMovies();
}

$('#searchForm').addEventListener('submit', (event) => {
  event.preventDefault();
  state.q = $('#query').value.trim();
  state.pagina = 1;
  loadMovies({ scroll: true });
});

$('#genre').addEventListener('change', (event) => {
  state.genero = event.target.value;
  state.pagina = 1;
  loadMovies();
});

$('#sort').addEventListener('change', (event) => {
  state.ordem = event.target.value;
  state.pagina = 1;
  loadMovies();
});

$('#clear').addEventListener('click', clearSearch);

init();
