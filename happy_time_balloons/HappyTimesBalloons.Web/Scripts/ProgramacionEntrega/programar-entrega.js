var ProgramarEntrega = (function () {
    var _urlHorarios;
    var _$contenedor;
    var _$fechaInput;
    var _$fechaError;
    var _$horariosError;

    function init(config) {
        _urlHorarios = config.urlHorarios;
        _$contenedor = document.getElementById('contenedorHorarios');
        _$fechaInput = document.getElementById('fechaEntrega');
        _$fechaError = document.getElementById('fechaError');
        _$horariosError = document.getElementById('horariosError');

        _$fechaInput.addEventListener('change', function () {
            cargarHorarios(_$fechaInput.value);
        });
    }

    function cargarHorarios(fecha) {
        _$fechaError.style.display = 'none';
        _$horariosError.style.display = 'none';
        _$contenedor.innerHTML = '<p class="text-muted small"><i class="bi bi-arrow-repeat me-1"></i>Cargando horarios…</p>';

        fetch(_urlHorarios + '?fecha=' + encodeURIComponent(fecha))
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.error) {
                    _$fechaError.textContent = data.error;
                    _$fechaError.style.display = 'block';
                    _$contenedor.innerHTML = '';
                    return;
                }
                renderHorarios(data);
            })
            .catch(function () {
                _$horariosError.textContent = 'Error al cargar los horarios. Intenta de nuevo.';
                _$horariosError.style.display = 'block';
                _$contenedor.innerHTML = '';
            });
    }

    function renderHorarios(horarios) {
        if (!horarios || horarios.length === 0) {
            _$contenedor.innerHTML = '<p class="text-muted small">No hay horarios disponibles para esta fecha.</p>';
            return;
        }

        var html = '';
        horarios.forEach(function (h) {
            var disabled = h.tieneCupo ? '' : 'disabled';
            var opacidad = h.tieneCupo ? '' : 'opacity-50';
            var badgeClass = h.tieneCupo ? 'bg-success' : 'bg-secondary';
            html += '<div class="form-check border rounded p-3 mb-2 ' + opacidad + '">' +
                '<input class="form-check-input" type="radio" name="HorarioEntregaId"' +
                ' id="h' + h.horarioEntregaId + '" value="' + h.horarioEntregaId + '" ' + disabled + '>' +
                '<label class="form-check-label w-100" for="h' + h.horarioEntregaId + '">' +
                '<strong>' + h.etiqueta + '</strong>' +
                '<span class="text-muted ms-2">' + h.horaInicio + ' – ' + h.horaFin + '</span>' +
                '<span class="float-end badge ' + badgeClass + '">' + h.cuposRestantes + ' cupos disponibles</span>' +
                '</label>' +
                '</div>';
        });
        _$contenedor.innerHTML = html;
    }

    return { init: init };
}());
