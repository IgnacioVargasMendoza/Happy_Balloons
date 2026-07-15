var Sinpe = (function () {
    'use strict';

    // ── Módulo: lista de pagos SINPE ──────────────────────────────────────────

    function abrirDetalle(id) {
        var modal = document.getElementById('modalDetalle');
        var cuerpo = document.getElementById('modalDetalleCuerpo');

        if (!modal || !cuerpo) { return; }

        cuerpo.innerHTML = '<div class="text-center py-3"><div class="spinner-border text-primary" role="status"></div></div>';

        var bsModal = bootstrap.Modal.getOrCreate(modal);
        bsModal.show();

        fetch('/Sinpe/Detalle/' + id, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) { return r.text(); })
            .then(function (html) { cuerpo.innerHTML = html; })
            .catch(function () { cuerpo.innerHTML = '<p class="text-danger">No se pudo cargar el detalle.</p>'; });
    }

    function init() {
        document.querySelectorAll('[data-sinpe-id]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                abrirDetalle(this.dataset.sinpeId);
            });
        });
    }

    // ── Módulo: checkout (zona de entrega + método de pago) ───────────────────

    var Checkout = {

        init: function () {
            Checkout.bindZonas();
            Checkout.bindMetodosPago();
            Checkout.actualizarTotal();
        },

        bindZonas: function () {
            document.querySelectorAll('.zona-radio').forEach(function (radio) {
                radio.addEventListener('change', function () {
                    document.querySelectorAll('.zona-card').forEach(function (card) {
                        card.classList.remove('border-brand');
                    });
                    this.closest('label').classList.add('border-brand');
                    Checkout.actualizarTotal();
                });
            });
        },

        bindMetodosPago: function () {
            document.querySelectorAll('.pago-radio').forEach(function (radio) {
                radio.addEventListener('change', function () {
                    document.querySelectorAll('.pago-card').forEach(function (card) {
                        card.classList.remove('border-brand');
                    });
                    this.closest('label').classList.add('border-brand');

                    var esSinpe = this.value === 'SINPE';
                    document.getElementById('panelSinpe').style.display = esSinpe ? '' : 'none';
                    document.getElementById('panelTarjeta').style.display = esSinpe ? 'none' : '';
                });
            });
        },

        actualizarTotal: function () {
            var hfSubtotal = document.getElementById('hfSubtotal');
            if (!hfSubtotal) { return; }

            var subtotal = parseFloat(hfSubtotal.value) || 0;
            var radio = document.querySelector('.zona-radio:checked');
            if (!radio) { return; }

            var costo = parseFloat(radio.closest('label').dataset.costo) || 0;
            var lblCostoEnvio = document.getElementById('lblCostoEnvio');
            var lblTotal = document.getElementById('lblTotal');

            if (lblCostoEnvio) { lblCostoEnvio.textContent = Checkout.formatColones(costo); }
            if (lblTotal) { lblTotal.textContent = Checkout.formatColones(subtotal + costo); }
        },

        formatColones: function (num) {
            return '₡' + Math.round(num).toLocaleString('es-CR');
        }
    };

    return {
        init: init,
        Checkout: Checkout
    };
})();
