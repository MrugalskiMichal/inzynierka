// Simple frontend logic: fetch agents and render table
const tbody = document.querySelector('#agentsTable tbody');
const statusEl = document.getElementById('status');
const refreshBtn = document.getElementById('refreshBtn');
const refreshSecondsInput = document.getElementById('refreshSeconds');
let timer = null;

async function fetchAgents() {
  statusEl.textContent = 'Loading...';
  try {
    const res = await fetch('/api/agents');
    if (!res.ok) throw new Error('Failed to fetch');
    const agents = await res.json();
    renderAgents(agents);
    statusEl.textContent = `Loaded ${agents.length} agents`;
  } catch (err) {
    statusEl.textContent = 'Error loading agents';
    console.error(err);
  }
}

function getMetric(agent, key) {
  // prefer top-level field, fall back to agent.metrics
  if (agent[key] !== undefined) return agent[key];
  if (agent.metrics && agent.metrics[key] !== undefined) return agent.metrics[key];
  return undefined;
}

function renderAgents(agents) {
  tbody.innerHTML = '';
  for (const a of agents) {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${escapeHtml(a.agentId ?? '')}</td>
      <td>${escapeHtml(a.serverAddress ?? '')}</td>
      <td>${escapeHtml(a.collectionIntervalSeconds ?? '')}</td>
      <td>${formatBool(getMetric(a, 'cpuUsage'))}</td>
      <td>${formatBool(getMetric(a, 'cpuTemperature'))}</td>
      <td>${formatBool(getMetric(a, 'ramUsage'))}</td>
      <td>${formatBool(getMetric(a, 'gpuUsage'))}</td>
      <td>${formatBool(getMetric(a, 'gpuTemperature'))}</td>
      <td>${formatBool(getMetric(a, 'diskUsage'))}</td>
      <td>${formatBool(getMetric(a, 'fanSpeeds'))}</td>
    `;
    tbody.appendChild(tr);
  }
}

function formatBool(v) {
  if (v === true) return '<span class="val true">✔</span>';
  if (v === false) return '<span class="val false">✖</span>';
  return '<span class="val unknown">—</span>';
}

function escapeHtml(s) {
  return String(s).replace(/[&<>"']/g, function (c) {
    return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
  });
}

function startAutoRefresh() {
  stopAutoRefresh();
  const secs = Math.max(1, parseInt(refreshSecondsInput.value) || 10);
  timer = setInterval(fetchAgents, secs * 1000);
}

function stopAutoRefresh() {
  if (timer) clearInterval(timer);
  timer = null;
}

refreshBtn.addEventListener('click', fetchAgents);
refreshSecondsInput.addEventListener('change', startAutoRefresh);

// initial load
fetchAgents();
startAutoRefresh();
