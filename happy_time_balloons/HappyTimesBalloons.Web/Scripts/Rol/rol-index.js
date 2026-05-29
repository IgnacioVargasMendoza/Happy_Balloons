document.addEventListener('DOMContentLoaded', function () {
    var modal = document.getElementById('modalEliminar');
    if (!modal) return;

    modal.addEventListener('show.bs.modal', function (event) {
        var boton = event.relatedTarget;
        var rolId = boton.getAttribute('data-rol-id');
        var rolNombre = boton.getAttribute('data-rol-nombre');

        document.getElementById('inputRolId').value = rolId;
        document.getElementById('nombreRolEliminar').textContent = rolNombre;
    });
});
