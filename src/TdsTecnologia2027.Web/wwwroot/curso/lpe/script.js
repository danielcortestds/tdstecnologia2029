(() => {
  const root = document.documentElement;
  const header = document.querySelector('.site-header');
  const progress = document.querySelector('.scroll-progress');
  const mobileBar = document.querySelector('.mobile-sticky');
  const hero = document.querySelector('.hero');
  const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
  const parallaxItems = [...document.querySelectorAll('[data-parallax]')];
  let queued = false;

  function updateScroll() {
    const y = window.scrollY;
    const maxScroll = Math.max(1, root.scrollHeight - window.innerHeight);
    progress.style.width = `${Math.min(100, y / maxScroll * 100)}%`;
    header.classList.toggle('scrolled', y > 35);
    mobileBar.classList.toggle('is-visible', y > hero.offsetHeight * .55);

    if (!reducedMotion.matches) {
      parallaxItems.forEach((item) => {
        const section = item.closest('section');
        const rect = section.getBoundingClientRect();
        if (rect.bottom < -100 || rect.top > window.innerHeight + 100) return;
        const centerOffset = (window.innerHeight / 2) - (rect.top + rect.height / 2);
        const amount = Math.max(-70, Math.min(70, centerOffset * Number(item.dataset.parallax)));
        item.style.setProperty('--parallax-y', `${amount.toFixed(1)}px`);
      });
    }
    queued = false;
  }
  function onScroll() {
    if (queued) return;
    queued = true;
    requestAnimationFrame(updateScroll);
  }
  window.addEventListener('scroll', onScroll, { passive: true });
  window.addEventListener('resize', onScroll, { passive: true });
  updateScroll();

  const reveals = document.querySelectorAll('.reveal');
  if ('IntersectionObserver' in window && !reducedMotion.matches) {
    const revealObserver = new IntersectionObserver((entries, observer) => {
      for (const entry of entries) {
        if (!entry.isIntersecting) continue;
        entry.target.classList.add('visible');
        observer.unobserve(entry.target);
      }
    }, { threshold: .09, rootMargin: '0px 0px -25px 0px' });
    reveals.forEach((element) => revealObserver.observe(element));
  } else {
    reveals.forEach((element) => element.classList.add('visible'));
  }

  const menuToggle = document.querySelector('.menu-toggle');
  const mobileMenu = document.querySelector('.mobile-menu');
  function closeMenu() {
    menuToggle.setAttribute('aria-expanded', 'false');
    menuToggle.setAttribute('aria-label', 'Abrir menu');
    mobileMenu.hidden = true;
    document.body.classList.remove('menu-open');
  }
  menuToggle.addEventListener('click', () => {
    const open = menuToggle.getAttribute('aria-expanded') !== 'true';
    menuToggle.setAttribute('aria-expanded', String(open));
    menuToggle.setAttribute('aria-label', open ? 'Fechar menu' : 'Abrir menu');
    mobileMenu.hidden = !open;
    document.body.classList.toggle('menu-open', open);
  });
  mobileMenu.querySelectorAll('a').forEach((link) => link.addEventListener('click', closeMenu));
  window.addEventListener('keydown', (event) => {
    if (event.key === 'Escape') closeMenu();
  });
  window.addEventListener('resize', () => {
    if (window.innerWidth > 800) closeMenu();
  });

  document.querySelectorAll('.module-list details').forEach((item) => {
    item.addEventListener('toggle', () => {
      if (!item.open) return;
      document.querySelectorAll('.module-list details').forEach((other) => {
        if (other !== item) other.open = false;
      });
    });
  });

  const videoFrame = document.querySelector('.video-frame');
  const videoUrl = videoFrame ? videoFrame.dataset.videoSrc.trim() : '';
  if (videoFrame && videoUrl) {
    const player = videoFrame.querySelector('.course-video');
    const preview = videoFrame.querySelector('.video-preview');
    const caption = document.querySelector('.video-stage-caption');
    player.addEventListener('error', () => {
      player.hidden = true;
      preview.hidden = false;
      caption.hidden = false;
    });
    player.src = videoUrl;
    player.hidden = false;
    preview.hidden = true;
    caption.hidden = true;
  }

  // O painel ilustrativo se monta quando entra na tela, uma única vez.
  // Sem JavaScript ou com movimento reduzido, os valores finais permanecem visíveis.
  const showcase = document.querySelector('.showcase-wrap');
  if (showcase && !reducedMotion.matches && 'IntersectionObserver' in window) {
    const values = [...showcase.querySelectorAll('[data-value]')];
    const donut = showcase.querySelector('.donut');
    const number = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 0 });
    const money = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    function formatValue(element, value) {
      switch (element.dataset.format) {
        case 'brl': return `R$ ${number.format(Math.round(value))}`;
        case 'brl-2': return `R$ ${money.format(value)}`;
        case 'percent': return `${Math.round(value)}%`;
        default: return number.format(Math.round(value));
      }
    }

    root.classList.add('dashboard-motion-ready');
    values.forEach((element) => { element.textContent = formatValue(element, 0); });
    donut.style.setProperty('--donut-progress', '0%');

    function animateDashboard() {
      showcase.classList.add('is-active');
      const startedAt = performance.now();
      const duration = 1550;
      function frame(now) {
        const elapsed = Math.min(1, (now - startedAt) / duration);
        const progress = 1 - Math.pow(1 - elapsed, 3);
        values.forEach((element) => {
          element.textContent = formatValue(element, Number(element.dataset.value) * progress);
        });
        donut.style.setProperty('--donut-progress', `${(82 * progress).toFixed(2)}%`);
        if (elapsed < 1) requestAnimationFrame(frame);
      }
      requestAnimationFrame(frame);
    }

    const dashboardObserver = new IntersectionObserver((entries, observer) => {
      if (!entries.some((entry) => entry.isIntersecting)) return;
      animateDashboard();
      observer.disconnect();
    }, { threshold: 0.18, rootMargin: '0px 0px -5% 0px' });
    dashboardObserver.observe(showcase);
  }
})();
