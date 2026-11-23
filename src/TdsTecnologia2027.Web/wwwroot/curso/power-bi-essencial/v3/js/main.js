/* Power BI Essencial - TDS: parallax, hero, certificado, dashboard e barra de compra */
(function () {
  'use strict';
  var PARALLAX = 1; // intensidade do parallax (0 desliga, 2 dobra)
  var reduce = false;
  try { reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches; } catch (e) {}

  /* Parallax no scroll + barra de compra fixa */
  var raf = null;
  function applyMotion() {
    raf = null;
    var vh = window.innerHeight || 800;
    var k = reduce ? 0 : PARALLAX;
    var els = document.querySelectorAll('[data-plx],[data-plx-x]');
    for (var i = 0; i < els.length; i++) {
      var el = els[i];
      var r = (el.parentElement || el).getBoundingClientRect();
      var p = (r.top + r.height / 2 - vh / 2) / vh;
      if (p > 1) p = 1; if (p < -1) p = -1;
      var y = (parseFloat(el.getAttribute('data-plx')) || 0) * p * k;
      var x = (parseFloat(el.getAttribute('data-plx-x')) || 0) * p * k;
      el.style.transform = (x || y) ? 'translate3d(' + x.toFixed(1) + 'px,' + y.toFixed(1) + 'px,0)' : '';
    }
    var bar = document.getElementById('buybar');
    var hero = document.getElementById('topo');
    if (bar && hero) {
      bar.style.transform = hero.getBoundingClientRect().bottom > vh * 0.45 ? 'translateY(110%)' : '';
    }
  }
  function onScroll() { if (!raf) raf = requestAnimationFrame(applyMotion); }
  window.addEventListener('scroll', onScroll, { passive: true });
  window.addEventListener('resize', onScroll);
  onScroll();

  /* Hero: camadas seguem o mouse */
  var hero = document.getElementById('topo');
  if (hero && !reduce) {
    hero.addEventListener('pointermove', function (e) {
      var r = hero.getBoundingClientRect();
      hero.style.setProperty('--mx', ((e.clientX - r.left) / r.width - 0.5).toFixed(3));
      hero.style.setProperty('--my', ((e.clientY - r.top) / r.height - 0.5).toFixed(3));
    });
    hero.addEventListener('pointerleave', function () {
      hero.style.removeProperty('--mx'); hero.style.removeProperty('--my');
    });
  }

  /* Certificado: inclinacao 3D */
  var cert = document.getElementById('certwrap');
  if (cert && !reduce) {
    cert.addEventListener('pointermove', function (e) {
      var r = cert.getBoundingClientRect();
      cert.style.setProperty('--ry', (((e.clientX - r.left) / r.width - 0.5) * 22).toFixed(2));
      cert.style.setProperty('--rx', (((e.clientY - r.top) / r.height - 0.5) * -16).toFixed(2));
    });
    cert.addEventListener('pointerleave', function () {
      cert.style.removeProperty('--rx'); cert.style.removeProperty('--ry');
    });
  }

  /* Dashboard ilustrativo: troca de semestre (dados ficticios) */
  var DATA = {
    1: { m: ['JAN', 'FEV', 'MAR', 'ABR', 'MAI', 'JUN'], v: [26, 32, 45, 39, 48, 58], receita: 'R$ 248.500', pedidos: '1.284', ticket: 'R$ 193,54', online: '62%', pres: '38%' },
    2: { m: ['JUL', 'AGO', 'SET', 'OUT', 'NOV', 'DEZ'], v: [52, 61, 57, 66, 74, 83], receita: 'R$ 393.200', pedidos: '1.936', ticket: 'R$ 203,10', online: '68%', pres: '32%' }
  };
  function text(id, v) { var el = document.getElementById(id); if (el) el.textContent = v; }
  function setSem(n) {
    var S = DATA[n];
    for (var i = 0; i < 6; i++) {
      var bar = document.querySelector('[data-bar="' + i + '"]');
      var val = document.querySelector('[data-bv="' + i + '"]');
      var mon = document.querySelector('[data-bm="' + i + '"]');
      if (bar) bar.style.height = Math.round(S.v[i] / 90 * 82) + '%';
      if (val) val.textContent = S.v[i];
      if (mon) mon.textContent = S.m[i];
    }
    text('kReceita', S.receita); text('kPedidos', S.pedidos); text('kTicket', S.ticket);
    text('onlinePct', S.online); text('presPct', S.pres);
    var ob = document.getElementById('onlineBar'), pb = document.getElementById('presBar');
    if (ob) ob.style.width = S.online;
    if (pb) pb.style.width = S.pres;
    [1, 2].forEach(function (k) {
      var b = document.getElementById('sem' + k); if (!b) return;
      var on = k === n;
      b.setAttribute('aria-pressed', on ? 'true' : 'false');
      b.style.background = on ? '#15130F' : '#FFFFFF';
      b.style.color = on ? '#F5C518' : '#15130F';
    });
  }
  var b1 = document.getElementById('sem1'), b2 = document.getElementById('sem2');
  var current = 1;
  if (b1) b1.addEventListener('click', function () { current = 1; setSem(1); });
  if (b2) b2.addEventListener('click', function () { current = 2; setSem(2); });

  /* Dashboard: animacao de entrada quando a secao aparece (uma vez) */
  function milhar(n) { return String(Math.round(n)).replace(/\B(?=(\d{3})+(?!\d))/g, '.'); }
  var NUM = { 1: { receita: 248500, pedidos: 1284, ticket: 193.54 }, 2: { receita: 393200, pedidos: 1936, ticket: 203.10 } };
  var dash = document.getElementById('dash');
  if (dash && !reduce && 'IntersectionObserver' in window) {
    var bars = [], i;
    for (i = 0; i < 6; i++) bars.push(document.querySelector('[data-bar="' + i + '"]'));
    var ob = document.getElementById('onlineBar'), pb = document.getElementById('presBar');
    var all = bars.concat([ob, pb]).filter(Boolean);
    // estado inicial zerado, sem transicao
    all.forEach(function (el) { el.style.transition = 'none'; });
    bars.forEach(function (el) { if (el) el.style.height = '0%'; });
    if (ob) ob.style.width = '0%'; if (pb) pb.style.width = '0%';
    text('kReceita', 'R$ 0'); text('kPedidos', '0'); text('kTicket', 'R$ 0,00');
    for (i = 0; i < 6; i++) { var v0 = document.querySelector('[data-bv="' + i + '"]'); if (v0) v0.textContent = '0'; }
    var io = new IntersectionObserver(function (entries) {
      for (var j = 0; j < entries.length; j++) {
        if (!entries[j].isIntersecting) continue;
        io.disconnect();
        var S = DATA[current], N = NUM[current];
        void dash.offsetWidth; // aplica o estado zerado antes de animar
        bars.forEach(function (el, n) {
          if (!el) return;
          el.style.transition = 'height .7s cubic-bezier(.2,.7,.2,1) ' + (n * 90) + 'ms';
          el.style.height = Math.round(S.v[n] / 90 * 82) + '%';
        });
        if (ob) { ob.style.transition = 'width .9s cubic-bezier(.2,.7,.2,1)'; ob.style.width = S.online; }
        if (pb) { pb.style.transition = 'width .9s cubic-bezier(.2,.7,.2,1) .15s'; pb.style.width = S.pres; }
        var t0 = performance.now(), dur = 1300;
        (function step(now) {
          var p = Math.min(1, (now - t0) / dur), e = 1 - Math.pow(1 - p, 3);
          text('kReceita', 'R$ ' + milhar(N.receita * e));
          text('kPedidos', milhar(N.pedidos * e));
          text('kTicket', 'R$ ' + (N.ticket * e).toFixed(2).replace('.', ','));
          for (var n = 0; n < 6; n++) { var v = document.querySelector('[data-bv="' + n + '"]'); if (v) v.textContent = Math.round(S.v[n] * e); }
          if (p < 1) requestAnimationFrame(step);
          else bars.forEach(function (el) { if (el) el.style.transition = 'height .55s cubic-bezier(.2,.7,.2,1)'; });
        })(t0);
        break;
      }
    }, { threshold: 0.3 });
    io.observe(dash);
  }

  /* Botoes de compra: repassa UTMs da pagina ao checkout e avisa o GTM/GA4 */
  var incoming = new URLSearchParams(window.location.search);
  var links = document.querySelectorAll('a[data-cta]');
  Array.prototype.forEach.call(links, function (a) {
    try {
      var u = new URL(a.href);
      incoming.forEach(function (val, key) { if (/^(utm_|gclid|fbclid)/.test(key)) u.searchParams.set(key, val); });
      a.href = u.toString();
    } catch (e) {}
    a.addEventListener('click', function () {
      window.dataLayer = window.dataLayer || [];
      window.dataLayer.push({ event: 'clique_comprar', curso: 'power-bi-essencial', posicao: a.getAttribute('data-cta') });
    });
  });
})();
