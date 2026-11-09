// ===== Trilha do curso (módulos e aulas do Power BI Essencial) =====
const modules = [
  ['Primeiros Passos no Power BI', 4, [
    'O que é BI e onde o Power BI se encaixa',
    'Instalando o Power BI Desktop e tour pela interface',
    'Interatividade: filtragem cruzada, realce e segmentações',
    'Explorando um relatório pronto: páginas, pesquisa e detalhamento'
  ]],
  ['Carregando e Preparando Dados (Power Query)', 5, [
    'Obter dados: importando um Excel',
    'Editor do Power Query: perfil, tipos e etapas aplicadas',
    'Transformações essenciais e a linguagem M',
    'Atualizando dados e resolvendo erros de carga',
    'Tabela Calendário: Ano, Mês e Trimestre'
  ]],
  ['Modelagem de Dados', 4, [
    'Exibição de Modelo: relacionamentos, cardinalidade e direção do filtro',
    'Tabela fato e tabelas dimensão (modelo estrela)',
    'Metas: tabelas com granularidade diferente',
    'Relacionamentos inativos, muitos-para-muitos e filtro nos dois sentidos'
  ]],
  ['Visualização de Dados', 6, [
    'Páginas, tela, grade e boas práticas de layout',
    'Cartão, gráfico de barras e de linhas',
    'Tabela, matriz e segmentação de dados',
    'Mapa, rosca e gráfico combinado: quando usar cada um',
    'Visuais de IA: árvore de decomposição, principais influenciadores e Copilot',
    'Temas, formatação condicional e formatação de números'
  ]],
  ['DAX — Cálculos e Medidas', 5, [
    'Medidas DAX: SUM, COUNTROWS, DISTINCTCOUNT, AVERAGE, MAX e MIN',
    'Colunas calculadas, medidas e tabela de medidas',
    'Variáveis, IF, SWITCH e parâmetros numéricos',
    'CALCULATE e inteligência de tempo: ano atual vs ano anterior',
    'Indicadores (bookmarks) e botões de navegação'
  ]],
  ['Projeto Final e Publicação', 6, [
    'Projeto final (parte 1): arquivo, tema e cabeçalho',
    'Projeto final (parte 2): cartões e gráficos com design moderno',
    'Projeto final (parte 3): página Vendedores, navegação e filtros sincronizados',
    'Publicando no Serviço do Power BI: workspace, assinatura e atualização',
    'Exportação, modelo PBIT e entrega do projeto',
    'Encerramento e próximos passos'
  ]]
];
document.querySelector('#modules').innerHTML = modules.map((m, i) =>
  `<details><summary><span class="num">0${i + 1}</span><span>${m[0]}<small>${m[1]} AULAS</small></span></summary><ul>${m[2].map(a => `<li>${a}</li>`).join('')}</ul></details>`
).join('');

// ===== Demonstração do relatório (troca de semestre) =====
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
