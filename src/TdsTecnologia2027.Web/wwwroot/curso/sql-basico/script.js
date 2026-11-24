// ===== Trilha do curso (módulos e aulas do SQL Básico) =====
const modules = [
  ['Preparando o ambiente', 3, [
    'Boas-vindas e o caminho até o BI',
    'Instalando o PostgreSQL',
    'DBeaver, conexão e a base do curso'
  ]],
  ['Banco de dados em 25 minutos', 3, [
    'Dado, banco de dados e SGBD',
    'Tabelas, linhas, colunas e tipos de dados',
    'Chaves e relacionamentos'
  ]],
  ['Consultando dados', 5, [
    'SELECT, FROM, AS e DISTINCT',
    'Ordenando e limitando: ORDER BY e LIMIT',
    'Filtrando com WHERE, AND e OR',
    'IN, BETWEEN, LIKE e IS NULL',
    'Exercícios do módulo 3'
  ]],
  ['Calculando e resumindo', 5, [
    'Colunas calculadas e funções de data',
    'CASE WHEN para criar faixas',
    'COUNT, SUM, AVG, MIN e MAX com GROUP BY',
    'HAVING: filtrando o resultado agrupado',
    'Exercícios do módulo 4'
  ]],
  ['Juntando tabelas', 4, [
    'Por que várias tabelas + INNER JOIN',
    'LEFT JOIN: o que está faltando',
    'JOIN com 3 ou mais tabelas',
    'Exercícios do módulo 5'
  ]],
  ['Criando e alterando dados', 2, [
    'CREATE TABLE com chave primária e estrangeira',
    'INSERT, UPDATE e DELETE'
  ]],
  ['Projeto final: SQL para BI', 3, [
    'Criando a view vw_vendas para o BI',
    'Respondendo 5 perguntas de negócio',
    'Encerramento e próximos passos'
  ]]
];
document.querySelector('#modules').innerHTML = modules.map((m, i) =>
  `<details><summary><span class="num">0${i + 1}</span><span>${m[0]}<small>${m[1]} AULAS</small></span></summary><ul>${m[2].map(a => `<li>${a}</li>`).join('')}</ul></details>`
).join('');

// ===== Editor de consultas (resultados reais da base do curso) =====
const queries = [
  {
    title: 'Clientes de Mato Grosso',
    sql: `SELECT nome, cidade, segmento
FROM clientes
WHERE uf = 'MT'
ORDER BY nome
LIMIT 5;`,
    cols: ['nome', 'cidade', 'segmento'], num: [],
    rows: [
      ['Armazém Bom Preço', 'Rondonópolis', 'Mercearia'],
      ['Armazém Dois Irmãos', 'Lucas do Rio Verde', 'Mercearia'],
      ['Armazém Esperança', 'Cuiabá', 'Mercearia'],
      ['Armazém Guaraná', 'Tangará da Serra', 'Mercearia'],
      ['Armazém Ipê Amarelo', 'Rondonópolis', 'Mercearia']
    ],
    explain: '<b>SELECT</b> escolhe as colunas, <b>WHERE</b> filtra as linhas e <b>ORDER BY</b> ordena. É a primeira coisa que você aprende no módulo 3.'
  },
  {
    title: 'Clientes por segmento',
    sql: `SELECT segmento,
       COUNT(*) AS clientes
FROM clientes
GROUP BY segmento
ORDER BY clientes DESC;`,
    cols: ['segmento', 'clientes'], num: [1],
    rows: [
      ['Mercearia', '67'], ['Supermercado', '59'], ['Restaurante', '33'], ['Padaria', '21'], ['Conveniência', '20']
    ],
    explain: '<b>GROUP BY</b> resume milhares de linhas em poucas, e <b>COUNT</b> conta cada grupo. É o tipo de resumo que todo relatório pede (módulo 4).'
  },
  {
    title: 'Faturamento por categoria',
    sql: `SELECT cat.nome AS categoria,
       SUM(i.quantidade * i.preco_unitario
           * (1 - i.desconto)) AS faturamento
FROM itens_pedido i
JOIN produtos pr   ON pr.id_produto = i.id_produto
JOIN categorias cat ON cat.id_categoria = pr.id_categoria
JOIN pedidos p     ON p.id_pedido = i.id_pedido
WHERE p.status <> 'Cancelado'
GROUP BY cat.nome
ORDER BY faturamento DESC
LIMIT 5;`,
    cols: ['categoria', 'faturamento'], num: [1],
    rows: [
      ['Congelados', '241.926,99'], ['Laticínios', '238.375,58'], ['Mercearia', '194.603,70'], ['Higiene Pessoal', '166.125,89'], ['Bebidas', '162.234,22']
    ],
    explain: '<b>JOIN</b> junta as tabelas de itens, produtos, categorias e pedidos numa resposta só. No módulo 5 você monta essa consulta passo a passo.'
  },
  {
    title: 'A view pronta para o BI',
    sql: `-- vw_vendas: uma linha por item vendido,
-- já com cliente, produto e valores
SELECT ano, mes,
       SUM(valor_liquido) AS faturamento
FROM vw_vendas
GROUP BY ano, mes
ORDER BY ano, mes
LIMIT 6;`,
    cols: ['ano', 'mes', 'faturamento'], num: [0, 1, 2],
    rows: [
      ['2024', '1', '20.360,55'], ['2024', '2', '27.887,20'], ['2024', '3', '33.473,65'], ['2024', '4', '34.181,59'], ['2024', '5', '43.197,85'], ['2024', '6', '35.759,89']
    ],
    explain: 'No projeto final você cria a <b>vw_vendas</b>, uma view que deixa os dados prontos para o Power BI ou o Qlik, e responde perguntas de negócio com ela.'
  }
];

const KW = /\b(SELECT|FROM|WHERE|ORDER BY|GROUP BY|LIMIT|AS|JOIN|ON|DESC|AND|OR)\b/g;
const FN = /\b(COUNT|SUM)\b/g;
function esc(s) { return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;'); }
function highlight(sql) {
  return sql.split('\n').map(line => {
    if (line.trim().startsWith('--')) return `<span class="c">${esc(line)}</span>`;
    return esc(line)
      .replace(/'[^']*'/g, m => `<span class="s">${m}</span>`)
      .replace(KW, '<span class="k">$1</span>')
      .replace(FN, '<span class="f">$1</span>');
  }).join('\n');
}

const tabs = document.querySelector('#ed-tabs');
tabs.innerHTML = '<small>CONSULTAS</small>' + queries.map((q, i) =>
  `<button type="button" data-q="${i}"><span>consulta_0${i + 1}.sql</span>${q.title}</button>`).join('');

function showQuery(n) {
  const q = queries[n];
  document.querySelector('#ed-title').textContent = q.title;
  document.querySelector('#ed-sql').innerHTML = highlight(q.sql);
  const head = '<tr>' + q.cols.map((c, i) => `<th class="${q.num.includes(i) ? 'n' : ''}">${c}</th>`).join('') + '</tr>';
  const body = q.rows.map((r, ri) => `<tr style="animation-delay:${ri * 60}ms">` + r.map((v, i) => `<td class="${q.num.includes(i) ? 'n' : ''}">${v}</td>`).join('') + '</tr>').join('');
  document.querySelector('#ed-table').innerHTML = `<thead>${head}</thead><tbody>${body}</tbody>`;
  document.querySelector('#ed-count').textContent = `${q.rows.length} linhas retornadas`;
  document.querySelector('#ed-explain').innerHTML = q.explain;
  tabs.querySelectorAll('button').forEach(b => {
    b.classList.toggle('selected', +b.dataset.q === n);
    b.setAttribute('aria-pressed', +b.dataset.q === n);
  });
}
let current = 0;
tabs.addEventListener('click', e => { const b = e.target.closest('button'); if (b) { current = +b.dataset.q; showQuery(current); } });
document.querySelector('#ed-run').addEventListener('click', () => showQuery(current));
showQuery(0);

// ===== Animação de entrada (respeita prefers-reduced-motion) =====
(() => {
  const preference = window.matchMedia('(prefers-reduced-motion: reduce)');
  if (preference.matches || !('IntersectionObserver' in window)) return;
  const targets = document.querySelectorAll('.why-intro,.why-grid>div,.project-head,.editor,.project-results,.curriculum-intro,.modules>details,.offer>div,.faq>div');
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
