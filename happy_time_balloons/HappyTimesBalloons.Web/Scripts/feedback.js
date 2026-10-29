(function () {
    'use strict';

    var screenshotData = null;

    function init() {
        var btnCapturar = document.getElementById('htbBtnCapturar');
        var btnQuitar = document.getElementById('htbBtnQuitar');
        var preview = document.getElementById('htbScreenshotPreview');
        var hiddenData = document.getElementById('htbScreenshotData');
        var form = document.getElementById('formFeedback');
        var alerta = document.getElementById('htbFeedbackAlerta');
        var btnEnviar = document.getElementById('htbBtnEnviar');

        if (!form) { return; }

        btnCapturar.addEventListener('click', function () {
            btnCapturar.disabled = true;
            btnCapturar.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Capturando…';

            var modal = document.getElementById('modalFeedback');
            modal.classList.add('d-none');

            html2canvas(document.body, { useCORS: true, scale: 0.6 }).then(function (canvas) {
                modal.classList.remove('d-none');
                screenshotData = canvas.toDataURL('image/png').split(',')[1];
                hiddenData.value = screenshotData;
                preview.src = 'data:image/png;base64,' + screenshotData;
                preview.classList.remove('d-none');
                btnQuitar.classList.remove('d-none');
                btnCapturar.innerHTML = '<i class="bi bi-camera me-1"></i>Volver a capturar';
                btnCapturar.disabled = false;
            }).catch(function () {
                modal.classList.remove('d-none');
                btnCapturar.innerHTML = '<i class="bi bi-camera me-1"></i>Capturar pantalla';
                btnCapturar.disabled = false;
            });
        });

        btnQuitar.addEventListener('click', function () {
            screenshotData = null;
            hiddenData.value = '';
            preview.src = '';
            preview.classList.add('d-none');
            btnQuitar.classList.add('d-none');
            btnCapturar.innerHTML = '<i class="bi bi-camera me-1"></i>Capturar pantalla';
        });

        form.addEventListener('submit', function (e) {
            e.preventDefault();
            alerta.innerHTML = '';
            btnEnviar.disabled = true;
            btnEnviar.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Enviando…';

            var data = new FormData(form);

            fetch('/Feedback/CrearIssue', {
                method: 'POST',
                body: data
            })
            .then(function (r) { return r.json(); })
            .then(function (res) {
                btnEnviar.disabled = false;
                btnEnviar.innerHTML = '<i class="bi bi-send me-1"></i>Enviar';

                if (res.exito) {
                    alerta.innerHTML =
                        '<div class="alert alert-success py-2 small mb-0">' +
                        '<i class="bi bi-check-circle me-1"></i>' +
                        '¡Feedback enviado! <a href="' + res.url + '" target="_blank" rel="noopener" class="alert-link">Ver issue en GitHub</a>' +
                        '</div>';
                    form.reset();
                    screenshotData = null;
                    hiddenData.value = '';
                    preview.classList.add('d-none');
                    btnQuitar.classList.add('d-none');
                    btnCapturar.innerHTML = '<i class="bi bi-camera me-1"></i>Capturar pantalla';
                } else {
                    alerta.innerHTML =
                        '<div class="alert alert-danger py-2 small mb-0">' +
                        '<i class="bi bi-exclamation-triangle me-1"></i>' +
                        (res.mensaje || 'Error al enviar el feedback.') +
                        '</div>';
                }
            })
            .catch(function () {
                btnEnviar.disabled = false;
                btnEnviar.innerHTML = '<i class="bi bi-send me-1"></i>Enviar';
                alerta.innerHTML =
                    '<div class="alert alert-danger py-2 small mb-0">' +
                    '<i class="bi bi-exclamation-triangle me-1"></i>Error de conexión.' +
                    '</div>';
            });
        });

        var modal = document.getElementById('modalFeedback');
        if (modal) {
            modal.addEventListener('hidden.bs.modal', function () {
                alerta.innerHTML = '';
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
}());
