window.renderEnergyChart = (labels, values) => {

    const canvas = document.getElementById('energyChart');

    if (!canvas)
        return;

    const existingChart =
        Chart.getChart(canvas);

    if (existingChart) {
        existingChart.destroy();
    }

    new Chart(canvas, {
        type: 'line',

        data: {
            labels: labels,

            datasets: [{
                label: 'Energy Consumption (W)',

                data: values,

                borderWidth: 3,

                tension: 0.3,

                fill: true
            }]
        },

        options: {
            responsive: true,

            plugins: {
                legend: {
                    display: true
                }
            }
        }
    });
};