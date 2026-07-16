var PrediccionDemanda = (function () {

    function initGrafico() {
        var contenedor = document.getElementById('prediccion-grafico-data');
        var canvas = document.getElementById('chartHistorial');

        if (!contenedor || !canvas) {
            return;
        }

        var etiquetas = JSON.parse(contenedor.getAttribute('data-etiquetas') || '[]');
        var valores = JSON.parse(contenedor.getAttribute('data-valores') || '[]');
        var prediccionRaw = contenedor.getAttribute('data-prediccion');
        var prediccion = prediccionRaw !== 'null' ? parseFloat(prediccionRaw) : null;

        var etiquetasGrafico = etiquetas.slice();
        var valoresHistorial = valores.slice();
        var valoresPrediccion = new Array(etiquetas.length).fill(null);

        if (prediccion !== null) {
            etiquetasGrafico.push('Proximo periodo (prediccion)');
            valoresHistorial.push(null);
            valoresPrediccion.push(prediccion);
        }

        new Chart(canvas, {
            type: 'line',
            data: {
                labels: etiquetasGrafico,
                datasets: [
                    {
                        label: 'Unidades vendidas',
                        data: valoresHistorial,
                        borderColor: 'rgba(13, 110, 253, 0.9)',
                        backgroundColor: 'rgba(13, 110, 253, 0.1)',
                        tension: 0.3,
                        fill: true,
                        pointRadius: 5,
                        pointHoverRadius: 7,
                        spanGaps: false
                    },
                    {
                        label: 'Prediccion',
                        data: valoresPrediccion,
                        borderColor: 'rgba(25, 135, 84, 0.9)',
                        backgroundColor: 'rgba(25, 135, 84, 0.2)',
                        borderDash: [6, 4],
                        tension: 0,
                        fill: false,
                        pointRadius: 7,
                        pointStyle: 'star',
                        pointHoverRadius: 10,
                        spanGaps: false
                    }
                ]
            },
            options: {
                responsive: true,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                plugins: {
                    legend: {
                        position: 'top'
                    },
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                if (ctx.parsed.y === null) {
                                    return null;
                                }
                                return ctx.dataset.label + ': ' + ctx.parsed.y.toFixed(1);
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Unidades'
                        }
                    }
                }
            }
        });
    }

    function init() {
        initGrafico();
    }

    return { init: init };

})();

PrediccionDemanda.init();
