const api = {
  async request(url, options = {}) {
    const response = await fetch(url, {
      headers: { 'Content-Type': 'application/json', ...(options.headers || {}) },
      ...options,
    });
    const data = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(data.message || 'Não foi possível concluir a operação.');
    return data;
  },
  getTools(params = '') { return this.request(`/api/tools${params}`); },
  createTool(data) { return this.request('/api/tools', { method: 'POST', body: JSON.stringify(data) }); },
  getLoans(status) { return this.request(`/api/loans?status=${status}`); },
  createLoan(data) { return this.request('/api/loans', { method: 'POST', body: JSON.stringify(data) }); },
  returnLoan(id, responsibleName) { return this.request(`/api/loans/${id}/return`, { method: 'PATCH', body: JSON.stringify({ responsibleName }) }); },
};

const state = { tools: [], activeLoans: [], allLoans: [] };
const $ = (selector) => document.querySelector(selector);
const $$ = (selector) => [...document.querySelectorAll(selector)];

function escapeHtml(value = '') {
  return String(value).replace(/[&<>'"]/g, (char) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[char]);
}

function formatDate(value) {
  if (!value) return '—';
  return new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value));
}

function showToast(message, error = false) {
  const toast = $('#toast');
  toast.textContent = message;
  toast.style.background = error ? '#b91c1c' : '#132238';
  toast.classList.remove('hidden');
  clearTimeout(showToast.timer);
  showToast.timer = setTimeout(() => toast.classList.add('hidden'), 3500);
}

function showView(viewId) {
  $$('.view').forEach((view) => view.classList.toggle('hidden', view.id !== viewId));
  $$('.nav-item').forEach((item) => item.classList.toggle('active', item.dataset.view === viewId));
  const titles = { dashboard: 'Visão geral', tools: 'Ferramentas', loans: 'Empréstimos', history: 'Histórico' };
  $('#page-title').textContent = titles[viewId];
}

function renderTools(displayTools = state.tools) {
  const body = $('#tools-table');
  body.innerHTML = displayTools.length ? displayTools.map((tool) => {
    const hasAvailable = tool.available_quantity > 0;
    return `<tr><td class="font-bold text-slate-600">${escapeHtml(tool.code)}</td><td><strong class="text-ink">${escapeHtml(tool.name)}</strong><br><span class="text-xs text-slate-500">${escapeHtml(tool.description || 'Sem descrição')}</span></td><td>${tool.total_quantity}</td><td class="font-bold">${tool.available_quantity}</td><td><span class="badge ${hasAvailable ? 'badge-green' : 'badge-amber'}">${hasAvailable ? 'Disponível' : 'Indisponível'}</span></td></tr>`;
  }).join('') : '<tr><td colspan="5" class="empty">Nenhuma ferramenta encontrada.</td></tr>';

  const available = state.tools.filter((tool) => tool.available_quantity > 0);
  $('#loan-tool').innerHTML = '<option value="">Selecione um item</option>' + available.map((tool) => `<option value="${tool.id}">${escapeHtml(tool.code)} — ${escapeHtml(tool.name)} (${tool.available_quantity} disp.)</option>`).join('');
}

function loanCard(loan, compact = false) {
  return `<div class="flex flex-col gap-3 py-4 sm:flex-row sm:items-center sm:justify-between">
    <div><div class="flex flex-wrap items-center gap-2"><strong class="text-ink">${escapeHtml(loan.tool_name)}</strong><span class="badge badge-amber">${loan.quantity} emprestado(s)</span></div><p class="mt-1 text-sm text-slate-600">${escapeHtml(loan.student_name)}${loan.student_registration ? ` · ${escapeHtml(loan.student_registration)}` : ''}</p><p class="mt-1 text-xs text-slate-400">Registrado por ${escapeHtml(loan.responsible_name)} em ${formatDate(loan.loaned_at)}</p></div>
    ${compact ? '' : `<button class="secondary-btn shrink-0" data-return-id="${loan.id}" data-return-label="${escapeHtml(loan.tool_name)} — ${escapeHtml(loan.student_name)}">Registrar devolução</button>`}
  </div>`;
}

function renderLoans() {
  $('#dashboard-loans').innerHTML = state.activeLoans.length ? state.activeLoans.slice(0, 4).map((loan) => loanCard(loan, true)).join('') : '<div class="empty">Nenhum empréstimo em aberto.</div>';
  $('#loans-list').innerHTML = state.activeLoans.length ? state.activeLoans.map((loan) => `<div class="rounded-2xl border border-slate-200 px-4">${loanCard(loan)}</div>`).join('') : '<div class="empty">Nenhum empréstimo em aberto.</div>';
  $('#history-table').innerHTML = state.allLoans.length ? state.allLoans.map((loan) => `<tr><td><strong>${escapeHtml(loan.tool_name)}</strong><br><span class="text-xs text-slate-500">${escapeHtml(loan.tool_code)}</span></td><td>${escapeHtml(loan.student_name)}</td><td>${formatDate(loan.loaned_at)}</td><td>${formatDate(loan.returned_at)}</td><td><span class="badge ${loan.status === 'DEVOLVIDO' ? 'badge-slate' : 'badge-amber'}">${loan.status === 'DEVOLVIDO' ? 'Devolvido' : 'Emprestado'}</span></td></tr>`).join('') : '<tr><td colspan="5" class="empty">Nenhuma movimentação registrada.</td></tr>';
}

function renderMetrics() {
  const total = state.tools.reduce((sum, tool) => sum + Number(tool.total_quantity), 0);
  const available = state.tools.reduce((sum, tool) => sum + Number(tool.available_quantity), 0);
  $('#metric-total').textContent = total;
  $('#metric-available').textContent = available;
  $('#metric-loaned').textContent = total - available;
  $('#metric-active').textContent = state.activeLoans.length;
}

async function refresh() {
  try {
    const [tools, activeLoans, allLoans] = await Promise.all([
      api.getTools(), api.getLoans('EMPRESTADO'), api.getLoans('TODOS'),
    ]);
    Object.assign(state, { tools, activeLoans, allLoans });
    const search = $('#tool-search').value;
    const status = $('#tool-filter').value;
    const visibleTools = search || status !== 'todos'
      ? await api.getTools(`?${new URLSearchParams({ search, status })}`)
      : tools;
    renderTools(visibleTools); renderLoans(); renderMetrics();
  } catch (error) {
    showToast(`${error.message} Verifique o servidor e o banco.`, true);
  }
}

function formData(form) { return Object.fromEntries(new FormData(form).entries()); }

$$('[data-view]').forEach((button) => button.addEventListener('click', () => showView(button.dataset.view)));
$$('[data-view-link]').forEach((button) => button.addEventListener('click', () => showView(button.dataset.viewLink)));
$$('[data-open]').forEach((button) => button.addEventListener('click', () => $(`#${button.dataset.open}`).showModal()));
$$('[data-close]').forEach((button) => button.addEventListener('click', () => button.closest('dialog').close()));

$('#tool-search').addEventListener('input', loadFilteredTools);
$('#tool-filter').addEventListener('change', loadFilteredTools);
let searchTimer;
function loadFilteredTools() {
  clearTimeout(searchTimer);
  searchTimer = setTimeout(async () => {
    try {
      const query = new URLSearchParams({ search: $('#tool-search').value, status: $('#tool-filter').value });
      const filteredTools = await api.getTools(`?${query}`);
      renderTools(filteredTools);
    } catch (error) { showToast(error.message, true); }
  }, 250);
}

$('#tool-form').addEventListener('submit', async (event) => {
  event.preventDefault();
  const form = event.currentTarget;
  try {
    await api.createTool(formData(form));
    form.reset(); form.closest('dialog').close();
    showToast('Ferramenta cadastrada com sucesso.'); await refresh();
  } catch (error) { showToast(error.message, true); }
});

$('#loan-form').addEventListener('submit', async (event) => {
  event.preventDefault();
  const form = event.currentTarget;
  try {
    await api.createLoan(formData(form));
    form.reset(); form.closest('dialog').close();
    showToast('Empréstimo registrado com sucesso.'); await refresh();
  } catch (error) { showToast(error.message, true); }
});

document.addEventListener('click', (event) => {
  const button = event.target.closest('[data-return-id]');
  if (!button) return;
  const form = $('#return-form');
  form.elements.loanId.value = button.dataset.returnId;
  $('#return-description').textContent = button.dataset.returnLabel;
  $('#return-modal').showModal();
});

$('#return-form').addEventListener('submit', async (event) => {
  event.preventDefault();
  const form = event.currentTarget;
  const data = formData(form);
  try {
    await api.returnLoan(data.loanId, data.responsibleName);
    form.reset(); form.closest('dialog').close();
    showToast('Devolução registrada com sucesso.'); await refresh();
  } catch (error) { showToast(error.message, true); }
});

refresh();
