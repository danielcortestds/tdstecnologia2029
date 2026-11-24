// ===== Trilha do curso (módulos e aulas do Claude Code Sem Programar) =====
const modules = [
  ['O que é e como preparar o ambiente', 5, [
    'Boas-vindas: do chat ao agente',
    'Vitrine: os 4 projetos que você vai criar',
    'Planos e custos: o que é preciso',
    'O GitHub sem medo: a sua pasta na nuvem',
    'Ligando o Claude Code ao GitHub'
  ]],
  ['Primeiro projeto: a calculadora de orçamento', 4, [
    'O primeiro pedido',
    'Modo plano: entender antes de fazer',
    'Aprovar a entrega e ver a calculadora no ar',
    'Errou? Desfazer sem medo'
  ]],
  ['Como pedir para um agente', 4, [
    'O briefing: peça como para um fornecedor',
    'Iterar: um ajuste de cada vez',
    'Quando der erro: descreva e peça investigação',
    'O arquivo de regras do projeto (CLAUDE.md)'
  ]],
  ['Projeto 2: automação de planilhas', 4, [
    'Juntar 12 planilhas em uma só',
    'Limpar os dados: duplicados, datas e nomes',
    'O relatório em Excel e o download',
    'Subiu a planilha, o relatório se atualiza sozinho'
  ]],
  ['Projeto 3: painel de indicadores', 3, [
    'Do arquivo de vendas ao painel',
    'Filtros e boas práticas de painel',
    'Atualizar o painel com dados novos'
  ]],
  ['Projeto 4: sistema de pedidos', 3, [
    'Planejando um sistema de verdade',
    'Usando o sistema e testando as regras',
    'Busca e exportação para Excel'
  ]],
  ['Compartilhar, versões e trabalhar de qualquer lugar', 3, [
    'Compartilhar com a equipe com segurança',
    'Histórico de versões: voltar no tempo',
    'Trabalhar de qualquer lugar: sessões e celular'
  ]],
  ['Indo além com segurança', 3, [
    'Skills: ensinar uma tarefa uma vez só',
    'Conectores: e-mail, agenda e arquivos',
    'Cuidados: dados, LGPD e quando chamar a TI'
  ]]
];
document.querySelector('#modules').innerHTML = modules.map((m, i) =>
  `<details><summary><span class="num">0${i + 1}</span><span>${m[0]}<small>${m[1]} AULAS</small></span></summary><ul>${m[2].map(a => `<li>${a}</li>`).join('')}</ul></details>`
).join('');

// ===== Demonstração: no manual x com o Claude Code =====
const tasks = [
  {
    badAsk: 'Orçamento para um cliente novo',
    badAnswer: 'Abrir a tabela de preços, copiar produto por produto, calcular o desconto na mão, procurar o frete da cidade, montar o documento e conferir tudo de novo.',
    goodAsk: '<b>Pedido:</b> crie uma calculadora de orçamento. Escolho o cliente e os produtos, ela aplica o desconto por quantidade e o frete da cidade e gera o PDF com o logo da empresa.',
    goodAnswer: '✓ Calculadora publicada, com link para a equipe\n✓ Desconto e frete aplicados sozinhos\n✓ PDF pronto para enviar ao cliente\n\nVocê confere o plano antes e aprova a entrega no final.'
  },
  {
    badAsk: '12 planilhas de vendas, uma por mês',
    badAnswer: 'Copiar e colar aba por aba, corrigir datas em formatos diferentes, apagar linhas repetidas e refazer o relatório do zero todo mês.',
    goodAsk: '<b>Pedido:</b> junte as 12 planilhas em uma só, corrija datas e nomes, tire os duplicados e gere um relatório em Excel com resumo por mês e por vendedor.',
    goodAnswer: '✓ Base única e limpa\n✓ Relatório em Excel com totais e gráfico\n✓ Chegou o mês novo? Basta subir a planilha e o relatório se atualiza sozinho.'
  },
  {
    badAsk: 'Reunião de resultados na segunda',
    badAnswer: 'Filtrar a planilha várias vezes, montar gráficos um a um e copiar tudo para a apresentação, que fica velha assim que chegam dados novos.',
    goodAsk: '<b>Pedido:</b> crie um painel de vendas com faturamento, pedidos e ticket médio, gráfico mês a mês e filtros de período, vendedor, cidade e categoria.',
    goodAnswer: '✓ Painel no navegador, também no celular\n✓ Filtros que atualizam tudo ao mesmo tempo\n✓ Resumo em texto com o melhor mês e a cidade que mais comprou'
  },
  {
    badAsk: 'Pedidos anotados em planilha e no WhatsApp',
    badAnswer: 'Cada vendedor anota de um jeito, ninguém sabe o status do pedido e, no fim do mês, alguém passa horas juntando tudo.',
    goodAsk: '<b>Pedido:</b> crie um sistema de pedidos com cadastro de cliente e produtos, regras de status, busca e exportação para Excel.',
    goodAnswer: '✓ Sistema com cadastro, regras e busca\n✓ Status de cada pedido à vista da equipe\n✓ Exportação para Excel com um clique'
  }
];
function renderTask(n) {
  const t = tasks[n];
  document.querySelector('#bad-ask').textContent = t.badAsk;
  document.querySelector('#bad-answer').textContent = t.badAnswer;
  document.querySelector('#good-ask').innerHTML = t.goodAsk;
  document.querySelector('#good-answer').textContent = t.goodAnswer;
  document.querySelectorAll('[data-task]').forEach(b => {
    b.classList.toggle('selected', +b.dataset.task === n);
    b.setAttribute('aria-pressed', +b.dataset.task === n);
  });
}
document.querySelectorAll('[data-task]').forEach(b => b.addEventListener('click', () => renderTask(+b.dataset.task)));
renderTask(0);

// ===== Animação de entrada (respeita prefers-reduced-motion) =====
(() => {
  const preference = window.matchMedia('(prefers-reduced-motion: reduce)');
  if (preference.matches || !('IntersectionObserver' in window)) return;
  const targets = document.querySelectorAll('.project-head,.chat-demo,.project-results,.audience-intro,.cards>div,.curriculum-intro,.modules>details,.offer>div,.faq>div');
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
