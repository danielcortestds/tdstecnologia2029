// ===== Trilha do curso (módulos e aulas do Python Essencial) =====
const modules = [
  ['Primeiros Passos no Python', 3, [
    'Por que Python e o que vamos construir',
    'Instalando o Python no Windows',
    'VS Code, extensão Python e o primeiro script'
  ]],
  ['Fundamentos da Linguagem', 3, [
    'Variáveis, tipos e f-strings',
    'Operadores e entrada de dados',
    'Decisões com if, elif e else'
  ]],
  ['Repetição e Coleções', 3, [
    'Listas e o laço for',
    'Dicionários: dados com nome',
    'while, break e tratamento de erros'
  ]],
  ['Organizando o Código', 3, [
    'Funções',
    'Módulos, pip e ambiente virtual',
    'Arquivos: TXT, CSV e JSON'
  ]],
  ['Dados com pandas e APIs', 4, [
    'pandas: lendo e explorando as vendas',
    'pandas: filtros, agrupamentos e KPIs',
    'Consumindo uma API pública (Tabela FIPE)',
    'Gráficos com matplotlib e exportação para Excel'
  ]],
  ['Projeto Guiado: Relatório Auto MT', 2, [
    'O script completo do Relatório Auto MT',
    'Painel Auto MT com Streamlit e próximos passos'
  ]],
  ['Projeto Final Passo a Passo', 6, [
    'Preparando o projeto: pastas, utils e dados',
    'fipe.py: buscando o preço na API',
    'fipe.py: cache para consultar a API uma vez só',
    'main.py e os indicadores',
    'relatorio.py: gráficos e Excel',
    'painel.py: o painel Streamlit do zero'
  ]]
];
document.querySelector('#modules').innerHTML = modules.map((m, i) =>
  `<details><summary><span class="num">0${i + 1}</span><span>${m[0]}<small>${m[1]} AULAS</small></span></summary><ul>${m[2].map(a => `<li>${a}</li>`).join('')}</ul></details>`
).join('');

// ===== Demonstração do painel (troca de semestre) =====
const periods = [
  { r: 'R$ 248.500', o: '1.284', t: 'R$ 193,54', v: [26, 32, 45, 39, 48, 59], m: ['JAN', 'FEV', 'MAR', 'ABR', 'MAI', 'JUN'] },
  { r: 'R$ 312.800', o: '1.506', t: 'R$ 207,70', v: [42, 46, 39, 58, 61, 67], m: ['JUL', 'AGO', 'SET', 'OUT', 'NOV', 'DEZ'] }
];
function render(n) {
  const d = periods[n];
  document.querySelector('#revenue').textContent = d.r;
  document.querySelector('#orders').textContent = d.o;
  document.querySelector('#ticket').textContent = d.t;
  document.querySelector('#bars').innerHTML = d.v.map(v => `<div style="height:${v / 70 * 100}%" data-value="${v}"></div>`).join('');
  document.querySelector('#months').innerHTML = d.m.map(m => `<span>${m}</span>`).join('');
  document.querySelectorAll('[data-period]').forEach(b => {
    b.classList.toggle('selected', +b.dataset.period === n);
    b.setAttribute('aria-pressed', +b.dataset.period === n);
  });
}
document.querySelectorAll('[data-period]').forEach(b => b.addEventListener('click', () => render(+b.dataset.period)));
render(0);

// ===== Animação de entrada (respeita prefers-reduced-motion) =====
(() => {
  const preference = window.matchMedia('(prefers-reduced-motion: reduce)');
  if (preference.matches || !('IntersectionObserver' in window)) return;
  const targets = document.querySelectorAll('.project-head,.dashboard,.project-results,.curriculum-intro,.modules>details,.offer>div,.faq>div');
  const observer = new IntersectionObserver(entries => {
    entries.forEach(entry => {
      if (entry.isIntersecting) { entry.target.classList.remove('tds-pending'); entry.target.classList.add('tds-visible'); observer.unobserve(entry.target); }
    });
  }, { threshold: 0, rootMargin: '0px 0px -35px 0px' });
  targets.forEach(el => {
    if (el.getBoundingClientRect().top < window.innerHeight) return;
    el.classList.add('tds-reveal', 'tds-pending');
    observer.observe(el);
  });
})();

// ===== Destino da compra (centralizado no config.js) =====
(() => {
  const config = window.TDS_CONFIG;
  if (!config) return;
  let destination;
  try { destination = new URL(config.checkoutUrl, window.location.href); } catch { return; }
  if (!['https:', 'http:'].includes(destination.protocol)) return;
  document.querySelectorAll('.tds-purchase').forEach(link => { link.href = destination.href; });
})();
