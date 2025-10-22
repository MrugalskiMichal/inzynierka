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
  return agent[key] ?? undefined;
}

function renderAgents(agents) {
  tbody.innerHTML = '';
  for (const a of agents) {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${escapeHtml(a.agentId ?? '')}</td>
      <td>${escapeHtml(a.serverAddress ?? '')}</td>
      <td>${escapeHtml(a.collectionIntervalSeconds ?? '')}</td>
      <td>${a.cpuUsage}</td>
      <td>${a.cpuTemperature}</td>
      <td>${a.ramUsage}</td>
      <td>${a.gpuUsage}</td>
      <td>${a.gpuTemperature}</td>
      <td>${a.diskUsage}</td>
      <td>${a.fanSpeeds}</td>
    `;
    tbody.appendChild(tr);
  }
}


function escapeHtml(s) {
  return String(s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
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
