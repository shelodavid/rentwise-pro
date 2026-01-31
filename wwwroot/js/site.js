(() => {
  const body = document.body;

  const sidebarToggle = document.querySelector('[data-sidebar-toggle]');
  if (sidebarToggle) {
    sidebarToggle.addEventListener('click', () => {
      body.classList.toggle('sidebar-open');
    });
  }

  const steppers = document.querySelectorAll('[data-stepper]');
  steppers.forEach((stepper) => {
    const panels = Array.from(stepper.querySelectorAll('[data-step-panel]'));
    const indicators = Array.from(stepper.querySelectorAll('[data-step-indicator]'));

    const setActiveStep = (index) => {
      panels.forEach((panel, panelIndex) => {
        const isActive = panelIndex === index;
        panel.classList.toggle('is-active', isActive);
        panel.setAttribute('aria-hidden', (!isActive).toString());
      });

      indicators.forEach((indicator, indicatorIndex) => {
        indicator.classList.toggle('is-active', indicatorIndex === index);
        indicator.classList.toggle('is-complete', indicatorIndex < index);
        indicator.setAttribute('aria-current', indicatorIndex === index ? 'step' : 'false');
      });

      stepper.dataset.activeStep = index.toString();
    };

    const initial = Number.parseInt(stepper.dataset.activeStep || '0', 10);
    setActiveStep(Number.isNaN(initial) ? 0 : initial);

    stepper.querySelectorAll('[data-step-next]').forEach((button) => {
      button.addEventListener('click', () => {
        const next = Math.min(panels.length - 1, Number(stepper.dataset.activeStep || 0) + 1);
        setActiveStep(next);
      });
    });

    stepper.querySelectorAll('[data-step-prev]').forEach((button) => {
      button.addEventListener('click', () => {
        const prev = Math.max(0, Number(stepper.dataset.activeStep || 0) - 1);
        setActiveStep(prev);
      });
    });
  });

  document.querySelectorAll('[data-collapse-target]').forEach((trigger) => {
    const targetId = trigger.getAttribute('data-collapse-target');
    if (!targetId) return;

    const panel = document.getElementById(targetId);
    if (!panel) return;

    trigger.addEventListener('click', () => {
      const isOpen = panel.classList.toggle('is-open');
      trigger.setAttribute('aria-expanded', isOpen.toString());
    });
  });
})();
