// Scripts/ranking-productos.js — lógica de gráficas del módulo Ranking de Productos
(function () {
    'use strict';

    var RankingProductos = {

        init: function () {
            var contenedor = document.getElementById('ranking-charts-data');
            if (!contenedor) {
                return;
            }

            var etiquetas    = JSON.parse(contenedor.getAttribute('data-etiquetas'));
            var unidades     = JSON.parse(contenedor.getAttribute('data-unidades'));
            var donaEtiquetas = JSON.parse(contenedor.getAttribute('data-dona-etiquetas'));
            var donaValores  = JSON.parse(contenedor.getAttribute('data-dona-valores'));

            RankingProductos.initChartBarras(etiquetas, unidades);
            RankingProductos.initChartDona(donaEtiquetas, donaValores);
        },

        initChartBarras: function (etiquetas, unidades) {
            var ctx = document.getElementById('chartBarras').getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: etiquetas,
                    datasets: [{
                        label: 'Unidades vendidas',
                        data: unidades,
                        backgroundColor: 'rgba(13, 110, 253, 0.7)',
                        borderColor: 'rgba(13, 110, 253, 1)',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    plugins: { legend: { display: false } },
                    scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                }
            });
        },

        initChartDona: function (etiquetas, valores) {
            var ctx = document.getElementById('chartDona').getContext('2d');
            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: etiquetas,
                    datasets: [{
                        data: valores,
                        backgroundColor: [
                            'rgba(13, 110, 253, 0.8)',
                            'rgba(25, 135, 84, 0.8)',
                            'rgba(13, 202, 240, 0.8)',
                            'rgba(255, 193, 7, 0.8)',
                            'rgba(220, 53, 69, 0.8)',
                            'rgba(111, 66, 193, 0.8)',
                            'rgba(253, 126, 20, 0.8)',
                            'rgba(32, 201, 151, 0.8)'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: { position: 'bottom' }
                    }
                }
            });
        }
    };

    // Sin jQuery en este proyecto — el script se carga al final del body, DOM ya disponible
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { RankingProductos.init(); });
    } else {
        RankingProductos.init();
    }

})();
