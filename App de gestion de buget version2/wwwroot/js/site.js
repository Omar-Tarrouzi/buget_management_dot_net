// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', () => {
    const symbols = ['$', '€', '£', '¥', '$$', '€€'];
    // Added Gold to the palette
    const colors = ['#21897E', '#3BA99C', '#69D1C5', '#85bb65', '#2ecc71', '#FFD700', '#FDB931'];

    function createMoneyParticle(x, y) {
        const particle = document.createElement('span');
        particle.classList.add('money-particle');
        particle.textContent = symbols[Math.floor(Math.random() * symbols.length)];

        // Randomize slight offset so they don't appear in a perfect line
        const offsetX = (Math.random() - 0.5) * 20;
        const offsetY = (Math.random() - 0.5) * 20;

        particle.style.left = `${x + offsetX}px`;
        particle.style.top = `${y + offsetY}px`;

        // Random color including gold
        const color = colors[Math.floor(Math.random() * colors.length)];
        particle.style.color = color;
        // Text shadow for gold
        if (color === '#FFD700' || color === '#FDB931') {
            particle.style.textShadow = '0 0 5px rgba(255, 215, 0, 0.5)';
            particle.style.zIndex = '10000';
        }

        // Random size
        const size = Math.random() * 1.2 + 0.8;
        particle.style.fontSize = `${size}rem`;

        document.body.appendChild(particle);

        // Cleanup
        setTimeout(() => {
            particle.remove();
        }, 1000);
    }

    // Rate limiter to prevent too many particles
    let lastTime = 0;
    const throttle = 40; // slightly faster for smoother burst feel

    document.addEventListener('mousemove', (e) => {
        // Expanded scope: cards, dashboard-cards, any white container, or the main container area
        const target = e.target.closest('.card, .dashboard-card, .bg-white, .container-fluid, .container');
        if (target) {
            const now = Date.now();
            if (now - lastTime > throttle) {
                createMoneyParticle(e.clientX, e.clientY);
                lastTime = now;
            }
        }
    });

    // Balance Animation Logic
    const balanceElements = document.querySelectorAll('.animate-balance');
    balanceElements.forEach(el => {
        const targetValue = parseFloat(el.getAttribute('data-value'));
        if (isNaN(targetValue)) return;

        let startValue = 0;
        const duration = 2000; // 2 seconds
        const startTime = performance.now();

        function update(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Ease out quart
            const ease = 1 - Math.pow(1 - progress, 4);

            const currentValue = startValue + (targetValue - startValue) * ease;

            // Format as currency (assuming EUR/default locale based on context, matches existing format)
            el.textContent = new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'EUR' }).format(currentValue);

            if (progress < 1) {
                requestAnimationFrame(update);
            } else {
                el.textContent = new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'EUR' }).format(targetValue);
            }
        }

        requestAnimationFrame(update);
    });
});
