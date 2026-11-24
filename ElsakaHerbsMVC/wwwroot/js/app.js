document.addEventListener("DOMContentLoaded", () => {
    const navLinks = [...document.querySelectorAll('.site-navbar .nav-link')];
    const sections = [...document.querySelectorAll('[data-section]')];
    const navCollapse = document.getElementById('mainNav');
    const isLandingPage = sections.length > 0;

    // Helper to extract the hash target (e.g., "#home" -> "home", "/#home" -> "home")
    function getHashTarget(link) {
        if (!link) return null;
        const href = link.getAttribute('href') || '';
        const hashIndex = href.indexOf('#');
        return hashIndex !== -1 ? href.substring(hashIndex + 1) : null;
    }

    // Set active link by section id
    function setActive(targetId) {
        if (!targetId) return;
        navLinks.forEach(link => {
            const hash = getHashTarget(link);
            const active = hash === targetId;
            link.classList.toggle('active', active);
            if (active) {
                link.setAttribute('aria-current', 'page');
            } else {
                link.removeAttribute('aria-current');
            }
        });
    }

    if (isLandingPage) {
        // 1. Initial State
        const currentHash = window.location.hash ? window.location.hash.substring(1) : '';
        if (currentHash && document.getElementById(currentHash)) {
            setActive(currentHash);
            // Slight delay to ensure layout has settled before smooth scrolling on hash arrival
            setTimeout(() => {
                const target = document.getElementById(currentHash);
                if (target) {
                    target.scrollIntoView({ behavior: 'smooth' });
                }
            }, 100);
        } else {
            setActive('home');
        }

        // 2. Scroll Spy using IntersectionObserver matching the original frontend
        // threshold 0 and rootMargin '-28% 0px -58% 0px' from original app.js
        const sectionObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (!entry.isIntersecting) return;
                // If user is at the very top, prioritize home
                if (window.scrollY < 80) {
                    setActive('home');
                    return;
                }
                const id = entry.target.id;
                setActive(id);
            });
        }, { rootMargin: '-28% 0px -58% 0px', threshold: 0 });

        sections.forEach(section => sectionObserver.observe(section));

        // Ensure "home" becomes active when scrolling back up to top
        window.addEventListener('scroll', () => {
            if (window.scrollY < 80) {
                setActive('home');
            }
        }, { passive: true });

        // 3. Click Navigation on Landing Page
        navLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                const targetId = getHashTarget(link);
                if (targetId) {
                    const targetEl = document.getElementById(targetId);
                    if (targetEl) {
                        e.preventDefault();
                        setActive(targetId);

                        // Smooth scroll to the target section
                        targetEl.scrollIntoView({ behavior: 'smooth' });

                        // Update URL hash without jumping
                        if (history.pushState) {
                            history.pushState(null, '', '#' + targetId);
                        } else {
                            window.location.hash = targetId;
                        }

                        // Close mobile navbar collapse menu if open
                        if (window.bootstrap && navCollapse && navCollapse.classList.contains('show')) {
                            bootstrap.Collapse.getOrCreateInstance(navCollapse).hide();
                        }
                    }
                }
            });
        });

        // Also handle the brand logo click on landing page
        const brand = document.querySelector('.site-navbar .navbar-brand');
        if (brand) {
            brand.addEventListener('click', (e) => {
                const homeEl = document.getElementById('home');
                if (homeEl) {
                    e.preventDefault();
                    setActive('home');
                    homeEl.scrollIntoView({ behavior: 'smooth' });
                    if (history.pushState) {
                        history.pushState(null, '', '#home');
                    }
                    if (window.bootstrap && navCollapse && navCollapse.classList.contains('show')) {
                        bootstrap.Collapse.getOrCreateInstance(navCollapse).hide();
                    }
                }
            });
        }

        // Also handle .nav-cta ("اطلب الآن") button in navbar if it targets #packages
        const navCta = document.querySelector('.site-navbar .nav-cta');
        if (navCta) {
            navCta.addEventListener('click', (e) => {
                const targetId = getHashTarget(navCta);
                if (targetId) {
                    const targetEl = document.getElementById(targetId);
                    if (targetEl) {
                        e.preventDefault();
                        setActive(targetId);
                        targetEl.scrollIntoView({ behavior: 'smooth' });
                        if (history.pushState) {
                            history.pushState(null, '', '#' + targetId);
                        }
                        if (window.bootstrap && navCollapse && navCollapse.classList.contains('show')) {
                            bootstrap.Collapse.getOrCreateInstance(navCollapse).hide();
                        }
                    }
                }
            });
        }
    } else {
        // Cross-Page considerations:
        // On non-landing pages (like /Checkout or /product/1), clicking links will navigate naturally to /#about etc.
        // Close mobile menu if clicked
        navLinks.forEach(link => {
            link.addEventListener('click', () => {
                if (window.bootstrap && navCollapse && navCollapse.classList.contains('show')) {
                    bootstrap.Collapse.getOrCreateInstance(navCollapse).hide();
                }
            });
        });
    }

    // 4. Back-to-top visibility & smooth scroll (works across all pages)
    const backToTop = document.getElementById('backToTop');
    if (backToTop) {
        const updateBackToTop = () => backToTop.classList.toggle('visible', window.scrollY > 500);
        window.addEventListener('scroll', updateBackToTop, { passive: true });
        updateBackToTop();
        backToTop.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));
    }
});
