// ===== Trilha do curso (módulos e aulas do QlikSense Essencial) =====
const modules = [
  ['Introdução ao Qlik Sense', 4, [
    'O que é BI e onde o Qlik Sense se encaixa',
    'Criando a conta no Qlik Cloud e tour pelo hub',
    'O modelo associativo: verde, branco e cinza',
    'Explorando um app: seleções, pesquisa e favoritos'
  ]],
  ['Carregando e preparando dados', 5, [
    'Criando um app e importando um Excel',
    'Gerenciador de Dados: perfis, associações e transformações',
    'Primeiro contato com o Editor de Script',
    'Recarregando dados e lendo o log de carga',
    'Calendário: Year(), Month() e campos derivados'
  ]],
  ['Modelagem de dados', 4, [
    'Visualizador do Modelo de Dados',
    'Tabela fato e tabelas dimensão: modelo estrela',
    'Chaves sintéticas: por que surgem e como resolver',
    'Referências circulares e QUALIFY/renomeação'
  ]],
  ['Visualizações e dashboards', 6, [
    'Pastas, grade e boas práticas de layout',
    'KPIs, gráficos de barras e de linhas',
    'Tabelas simples, dinâmicas e painel de filtros',
    'Mapa, pizza e combo: quando usar cada um',
    'Qlik Answers: perguntas em linguagem natural',
    'Temas, cores e formatação de números'
  ]],
  ['Expressões e análise avançada', 5, [
    'Funções de agregação: Sum, Count, Avg, Max, Min',
    'Itens mestres: dimensões, medidas e visualizações',
    'Variáveis e If() nas expressões',
    'Introdução a Set Analysis: ano atual vs. anterior',
    'Marcadores e seleções salvas'
  ]],
  ['Compartilhamento e projeto final', 4, [
    'Compartilhando o app: espaços, assinaturas e alertas',
    'Histórias e exportação para PDF',
    'Exportar e importar o app (QVF)',
    'Projeto final e próximos passos'
  ]]
];
document.querySelector('#modules').innerHTML = modules.map((m, i) =>
  `<details><summary><span class="num">0${i + 1}</span><span>${m[0]}<small>${m[1]} AULAS</small></span></summary><ul>${m[2].map(a => `<li>${a}</li>`).join('')}</ul></details>`
).join('');

// ===== Demonstração do dashboard (troca de semestre) =====
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
