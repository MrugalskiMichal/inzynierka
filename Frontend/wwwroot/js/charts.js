window.renderLineChart = (canvasId, labels, data, label, color, options) => {
    console.debug('renderLineChart called', canvasId, label, labels?.length, data?.length, options);
    const el = document.getElementById(canvasId);
    // if chart instance exists but element is missing or replaced, destroy old instance
    const chartKey = canvasId + '_chart';
    if (window[chartKey] && (!el || window[chartKey].canvas !== el)) {
        try { window[chartKey].destroy(); } catch (e) { console.warn('destroy failed', e); }
        delete window[chartKey];
    }
    if (!el) return;
    const ctx = el.getContext('2d');
    if (window[chartKey]) {
        window[chartKey].data.labels = labels;
        window[chartKey].data.datasets[0].data = data;
        if (options && options.yMax !== undefined) { 
            window[chartKey].options.scales.y.max = options.yMax; }
        window[chartKey].update();
        return;
    }

    const yOptions = { beginAtZero: true };
    if (options && options.yMax !== undefined) {
        yOptions.max = options.yMax;
    }

    window[canvasId + '_chart'] = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: data,
                borderColor: color || '#007bff',
                backgroundColor: 'rgba(0,0,0,0)',
                tension: 0.2,
                pointRadius: 3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: yOptions }
        }
    });
};

window.renderBarChart = (canvasId, labels, data, label, color, options) => {
    const el = document.getElementById(canvasId);
    if (!el) return;

    const ctx = el.getContext('2d');
    const chartKey = canvasId + '_chart';

    if (window[chartKey]) {
        try { window[chartKey].destroy(); } catch {}
        delete window[chartKey];
    }

    const yOptions = { beginAtZero: true };
    if (options && options.yMax !== undefined) {
        yOptions.max = options.yMax;
    }

    window[chartKey] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: data,
                backgroundColor: color + "AA",
                borderColor: color,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: yOptions }
        }
    });
};


window.renderMultipleCharts = (containerId, seriesObject, labels, options, fallbackSeries) => {
    console.debug('renderMultipleCharts called', containerId, seriesObject, labels?.length, Object.keys(seriesObject || {}).length, options, fallbackSeries?.length);
    const container = document.getElementById(containerId);
    if (!container) { console.warn('renderMultipleCharts: container not found', containerId); return; }
    // destroy any existing chart instances inside this container to avoid stale references
    Array.from(container.querySelectorAll('canvas')).forEach(c => {
        const key = c.id + '_chart';
        if (window[key]) {
            try { window[key].destroy(); } catch (e) { console.warn('destroy failed', e); }
            delete window[key];
        }
    });
    container.innerHTML = '';

    const palette = ['#3498db', '#e74c3c', '#2ecc71', '#9b59b6', '#f39c12', '#1abc9c'];
    let idx = 0;
    let hasAny = false;
        for (const key in seriesObject) {
        hasAny = true;
        console.debug('renderMultipleCharts: series', key, seriesObject[key]?.length);
        const canvas = document.createElement('canvas');
        const safeId = containerId + '_' + key.replace(/[^a-zA-Z0-9]/g, '_');
        canvas.id = safeId;
        canvas.style.width = '340px';
        canvas.style.height = '200px';
            canvas.style.display = 'block';
        canvas.className = 'me-2 mb-2';
        container.appendChild(canvas);

        const color = palette[idx % palette.length];
        idx++;

        window.renderLineChart(safeId, labels, seriesObject[key], key, color, options);
    }
    // if no per-disk series, but a fallbackSeries array was provided, render a single chart
    if (!hasAny && Array.isArray(fallbackSeries) && fallbackSeries.length > 0) {
        console.debug('renderMultipleCharts: rendering fallback overall series, length=', fallbackSeries.length);
        const canvas = document.createElement('canvas');
        const safeId = containerId + '_overall';
        canvas.id = safeId;
        canvas.style.width = '700px';
        canvas.style.height = '180px';
        canvas.className = 'me-2 mb-2';
        container.appendChild(canvas);
        window.renderLineChart(safeId, labels, fallbackSeries, 'Disk % (overall)', '#3498db', options);
    }
};
