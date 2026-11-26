// ===== Trilha do curso (módulos e aulas do Power BI Avançado) =====
const modules = [
  ['O novo jeito de trabalhar', 2, [
    'Salvando como Projeto do Power BI (PBIP)',
    'Exibição de consulta DAX: testar antes de criar'
  ]],
  ['DAX avançado', 4, [
    'Contexto de linha, contexto de filtro e transição de contexto',
    'Iteradores e controle de filtros: SUMX, REMOVEFILTERS, RANKX e TOPN',
    'Funções DAX definidas pelo usuário (UDF)',
    'Grupos de cálculo: um comparativo para todas as medidas'
  ]],
  ['Análises dinâmicas', 3, [
    'Cálculos visuais: acumulado, média móvel e % do ano',
    'Parâmetros de campo: o usuário escolhe métrica e dimensão',
    'Inteligência de tempo com calendários: o ano-safra'
  ]],
  ['Segurança e desempenho', 2, [
    'Segurança em nível de linha (RLS) dinâmica',
    'Desempenho: Analisador de desempenho e DAX que escala'
  ]],
  ['Power BI com IA', 3, [
    'Modelo pronto para IA: descrições, pastas e Copilot',
    'Claude + Power BI: conectando pelo MCP',
    'Projeto final: Painel Executivo Auto MT e encerramento'
  ]]
];
document.querySelector('#modules').innerHTML = modules.map((m, i) =>
  `<details><summary><span class="num">0${i + 1}</span><span>${m[0]}<small>${m[1]} AULAS</small></span></summary><ul>${m[2].map(a => `<li>${a}</li>`).join('')}</ul></details>`
).join('');

// ===== Demonstração do painel (ano civil x ano-safra) =====
const periods = [
  { r: 'R$ 18,4 mi', o: '+12,6%', t: 'R$ 146,2 mil', v: [12, 22, 35, 47, 58, 70], m: ['JAN–FEV', 'MAR–ABR', 'MAI–JUN', 'JUL–AGO', 'SET–OUT', 'NOV–DEZ'] },
  { r: 'R$ 19,1 mi', o: '+15,3%', t: 'R$ 149,8 mil', v: [14, 25, 36, 46, 59, 70], m: ['JUL–AGO', 'SET–OUT', 'NOV–DEZ', 'JAN–FEV', 'MAR–ABR', 'MAI–JUN'] }
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
