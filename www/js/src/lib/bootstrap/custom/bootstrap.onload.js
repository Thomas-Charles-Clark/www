'use strict';

//#region Hotspot Init

(() => {
    const popperOptions = {
        modifiers: [
            {
                name: 'offset',
                options: {
                    offset: [0, -25],
                },
            },
            {
                name: 'preventOverflow',
                options: {

                }
            }
        ],
    };

    const showEvents = ['mouseenter', 'focus'];
    const hideEvents = ['mouseleave', 'blur'];

    document.querySelectorAll('.hotspot-overlay').forEach(overlay => {
        window.setTimeout(() => {
            let marker = overlay.parentNode;

            let popperInstance = bootstrap.Popper.createPopper(marker, overlay, popperOptions)

            function show() {
                // Make the tooltip visible
                overlay.setAttribute('data-show', '');

                // Enable the event listeners
                popperInstance.setOptions((options) => ({
                    ...options,
                    modifiers: [
                        ...options.modifiers,
                        {
                            name: 'eventListeners', enabled: true
                        },
                        {
                            name: 'preventOverflow',
                            options: {
                                boundary: overlay.closest('.page-section')
                            }
                        }
                    ],
                }));

                // Update its position
                popperInstance.update();
            }

            function hide() {
                // Hide the tooltip
                overlay.removeAttribute('data-show');

                // Disable the event listeners
                popperInstance.setOptions((options) => ({
                    ...options,
                    modifiers: [
                        ...options.modifiers,
                        { name: 'eventListeners', enabled: false },
                    ],
                }));
            }

            showEvents.forEach((event) => {
                overlay.parentNode.addEventListener(event, show);
            });

            hideEvents.forEach((event) => {
                overlay.parentNode.addEventListener(event, hide);
            });
        }, 0);
    });
})();

//#endregion

if (typeof window.onBootstrapLoad === 'function') {
    console.log('window.onBootstrapLoad');
    window.onBootstrapLoad();
}