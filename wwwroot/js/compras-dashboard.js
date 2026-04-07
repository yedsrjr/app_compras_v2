(() => {
  const panel = document.getElementById('analysisPanel');
  const toggle = document.getElementById('toggleAnalysis');
  const updateLabel = () => {
    if (!toggle || !panel) return;
    toggle.textContent = panel.classList.contains('hidden') ? 'Mostrar analise' : 'Ocultar analise';
  };
  if (panel && toggle) {
    toggle.addEventListener('click', () => {
      panel.classList.toggle('hidden');
      updateLabel();
    });
    updateLabel();
  }

  const data = Array.isArray(window.menorValorData) ? window.menorValorData : [];
  const requiresItem = Boolean(window.dashboardRequiresItem);
  if (!data.length) {
    if (toggle) {
      toggle.disabled = true;
      toggle.textContent = 'Analise indisponivel';
    }
    return;
  }

  const filters = window.analysisFilters || {};
  const fmtCurrency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });
  const fmtDate = new Intl.DateTimeFormat('pt-BR');

  const toNumber = (value) => {
    if (typeof value === 'number') return Number.isFinite(value) ? value : 0;
    if (value == null) return 0;
    const str = String(value).trim();
    if (!str) return 0;
    const normalized = str.replace(/\./g, '').replace(',', '.');
    const parsed = Number.parseFloat(normalized);
    return Number.isFinite(parsed) ? parsed : 0;
  };

  const normalizeDate = (value) => {
    if (!value) return null;
    const d = new Date(value);
    return Number.isNaN(d.getTime()) ? null : d;
  };

  const normalized = data.map(x => {
    const desc = x.Description || x.descricao || '';
    const parts = desc.split(' - ');
    const code = parts.length >= 2 ? parts[0].trim() : '';
    const name = parts.length >= 2 ? parts.slice(1).join(' - ').trim() : desc.trim();
    const itemKey = code ? `${code} — ${name}` : name;
    const fornecedor = x.Fornecedor || x.mv_fornecedor || '';
    const codPc = x.CodPc ?? x.Cod_PC ?? '';
    const dataPc = x.DataPc ?? x.data_pc ?? null;
    const quantidade = x.Quantidade ?? x.qtd_pc ?? 0;
    const precoUnit = x.PrecoUnitario ?? x.preco_unit ?? 0;
    const valorTotal = x.ValorTotalItem ?? x.valor_total_item ?? 0;
    return {
      ...x,
      FornecedorNorm: fornecedor,
      CodPcNorm: codPc,
      QuantidadeNum: toNumber(quantidade),
      PrecoUnitarioNum: toNumber(precoUnit),
      ValorTotalItemNum: toNumber(valorTotal),
      DataPcDate: normalizeDate(dataPc),
      ItemKey: itemKey,
      ItemCode: code,
      ItemName: name
    };
  });

  const filterSelect = document.getElementById('dashboardItemFilter');
  if (filterSelect) {
    filterSelect.addEventListener('change', () => {
      const selected = filterSelect.value;
      if (!selected) return;
      const params = new URLSearchParams(window.location.search);
      params.set('tipoBusca', 'item');
      params.set('valorBusca', selected);
      const dataInicio = filters.dataInicio || '';
      const dataFim = filters.dataFim || '';
      if (dataInicio) params.set('dataInicio', dataInicio);
      if (dataFim) params.set('dataFim', dataFim);
      params.set('tab', 'dashboard');
      window.location.search = params.toString();
    });
  }

  if (requiresItem) {
    return;
  }

  let lineChartInstance = null;
  let supplierChartInstance = null;
  let dashboardCurrentPage = 1;

  const dashboardPageSizeSelect = document.getElementById('dashboardPageSizeSelect');
  const dashboardPrevPage = document.getElementById('dashboardPrevPage');
  const dashboardNextPage = document.getElementById('dashboardNextPage');
  const dashboardPageInfo = document.getElementById('dashboardPageInfo');

  const getDashboardPageSize = () => parseInt(dashboardPageSizeSelect?.value || '10', 10);

  const renderDashboardPagination = (rows) => {
    if (!dashboardPageInfo || !dashboardPrevPage || !dashboardNextPage) return;
    const pageSize = getDashboardPageSize();
    const totalPages = Math.max(1, Math.ceil(rows.length / pageSize));
    dashboardCurrentPage = Math.min(dashboardCurrentPage, totalPages);

    rows.forEach((row, index) => {
      const start = (dashboardCurrentPage - 1) * pageSize;
      const end = start + pageSize;
      row.style.display = index >= start && index < end ? '' : 'none';
    });

    dashboardPageInfo.textContent = `Página ${dashboardCurrentPage} de ${totalPages}`;
    dashboardPrevPage.disabled = dashboardCurrentPage <= 1;
    dashboardNextPage.disabled = dashboardCurrentPage >= totalPages;
  };

  if (dashboardPageSizeSelect) {
    dashboardPageSizeSelect.addEventListener('change', () => {
      dashboardCurrentPage = 1;
      const rows = Array.from(document.querySelectorAll('#analysisTableBody tr'));
      renderDashboardPagination(rows);
    });
  }

  if (dashboardPrevPage) {
    dashboardPrevPage.addEventListener('click', () => {
      dashboardCurrentPage = Math.max(1, dashboardCurrentPage - 1);
      const rows = Array.from(document.querySelectorAll('#analysisTableBody tr'));
      renderDashboardPagination(rows);
    });
  }

  if (dashboardNextPage) {
    dashboardNextPage.addEventListener('click', () => {
      dashboardCurrentPage = dashboardCurrentPage + 1;
      const rows = Array.from(document.querySelectorAll('#analysisTableBody tr'));
      renderDashboardPagination(rows);
    });
  }

  const periodText = (filters.dataInicio || filters.dataFim)
    ? `${filters.dataInicio || '...'} → ${filters.dataFim || '...'}`
    : 'Ultimos 90 dias';

  const setText = (id, value) => {
    const el = document.getElementById(id);
    if (el) el.textContent = value;
  };

  const updateHeader = (items) => {
    const itemKeys = Array.from(new Set(items.map(x => x.ItemKey)));
    const title = itemKeys.length === 1 ? itemKeys[0] : 'Todos os itens';
    const suppliers = Array.from(new Set(items.map(x => x.FornecedorNorm).filter(Boolean)));
    const supplierLabel = suppliers.length === 1 ? suppliers[0] : 'Varios fornecedores';
    setText('analysisItemTitle', title || 'Item');
    setText('analysisSubTitle', `${supplierLabel} · ${periodText}`);
    setText('analysisCount', `${items.length} itens`);
  };

  const render = (items) => {
    if (!items.length) {
      updateHeader(items);
      setText('metricAvg', 'R$ 0,00');
      setText('metricMin', 'R$ 0,00');
      setText('metricMax', 'R$ 0,00');
      setText('metricVar', '0,0%');
      setText('metricMinMeta', '-');
      setText('metricMaxMeta', '-');
      const tbody = document.getElementById('analysisTableBody');
      if (tbody) tbody.innerHTML = '';
      return;
    }

    const sorted = items.slice().sort((a, b) => {
      const da = a.DataPcDate?.getTime() || 0;
      const db = b.DataPcDate?.getTime() || 0;
      return da - db;
    });
    const sortedDesc = items.slice().sort((a, b) => {
      const da = a.DataPcDate?.getTime() || 0;
      const db = b.DataPcDate?.getTime() || 0;
      return db - da;
    });

    const qtyTotal = sorted.reduce((sum, x) => sum + x.QuantidadeNum, 0);
    const weightedSum = sorted.reduce((sum, x) => sum + (x.PrecoUnitarioNum * x.QuantidadeNum), 0);
    const simpleAvg = sorted.reduce((sum, x) => sum + x.PrecoUnitarioNum, 0) / (sorted.length || 1);
    const avg = qtyTotal > 0 ? (weightedSum / qtyTotal) : simpleAvg;

    const minItem = sorted.reduce((min, x) => x.PrecoUnitarioNum < min.PrecoUnitarioNum ? x : min, sorted[0]);
    const maxItem = sorted.reduce((max, x) => x.PrecoUnitarioNum > max.PrecoUnitarioNum ? x : max, sorted[0]);
    const variation = minItem.PrecoUnitarioNum ? ((maxItem.PrecoUnitarioNum - minItem.PrecoUnitarioNum) / minItem.PrecoUnitarioNum) * 100 : 0;

    updateHeader(sorted);

    setText('metricAvg', fmtCurrency.format(avg || 0));
    setText('metricAvgUnit', 'por unidade');
    setText('metricMin', fmtCurrency.format(minItem.PrecoUnitarioNum || 0));
    setText('metricMinMeta', `${minItem.DataPcDate ? fmtDate.format(minItem.DataPcDate) : '-'} · PC ${minItem.CodPcNorm}`);
    setText('metricMax', fmtCurrency.format(maxItem.PrecoUnitarioNum || 0));
    setText('metricMaxMeta', `${maxItem.DataPcDate ? fmtDate.format(maxItem.DataPcDate) : '-'} · PC ${maxItem.CodPcNorm}`);
    setText('metricVar', `${variation.toFixed(1)}%`);

    const lineLabels = sorted.map(x => `PC ${x.CodPcNorm}\n${x.DataPcDate ? fmtDate.format(x.DataPcDate) : '-'}`);
    const lineValues = sorted.map(x => x.PrecoUnitarioNum || 0);
    const minVal = Math.min(...lineValues);
    const maxVal = Math.max(...lineValues);
    const padding = (maxVal - minVal) * 0.1;
    const yMin = Math.max(0, minVal - padding);
    const yMax = maxVal + padding;

    const lineChartEl = document.getElementById('lineChart');
    if (lineChartEl && window.Chart) {
      if (lineChartInstance) lineChartInstance.destroy();
      lineChartInstance = new Chart(lineChartEl, {
        type: 'line',
        data: {
          labels: lineLabels,
          datasets: [
            {
              data: lineValues,
              borderColor: '#378ADD',
              backgroundColor: 'rgba(55,138,221,0.10)',
              borderWidth: 2,
              pointRadius: 4,
              pointBackgroundColor: '#378ADD',
              fill: true,
              tension: 0.3
            },
            {
              data: lineValues.map(() => avg),
              borderColor: '#E24B4A',
              borderWidth: 1.5,
              borderDash: [4, 4],
              pointRadius: 0,
              fill: false
            }
          ]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: ctx => fmtCurrency.format(ctx.parsed.y || 0)
              }
            }
          },
          scales: {
            y: {
              min: yMin,
              max: yMax,
              ticks: {
                font: { size: 10 },
                callback: v => fmtCurrency.format(v)
              },
              grid: { color: 'rgba(128,128,128,0.1)' }
            },
            x: {
              ticks: { display: false },
              grid: { display: false }
            }
          }
        }
      });
    }

    const legend = document.getElementById('lineLegend');
    if (legend) {
      legend.innerHTML = `
        <span class="legend-item">
          <span class="legend-line legend-blue"></span>Preco unitario
        </span>
        <span class="legend-item">
          <span class="legend-line legend-red"></span>Preco medio ${fmtCurrency.format(avg || 0)}
        </span>
      `;
    }

    const supplierGroups = new Map();
    sorted.forEach(x => {
      const key = x.FornecedorNorm || 'Fornecedor';
      if (!supplierGroups.has(key)) supplierGroups.set(key, { qty: 0, total: 0 });
      const group = supplierGroups.get(key);
      group.qty += x.QuantidadeNum;
      group.total += x.PrecoUnitarioNum * x.QuantidadeNum;
    });
    const supplierLabels = Array.from(supplierGroups.keys());
    const supplierAvgs = supplierLabels.map(label => {
      const g = supplierGroups.get(label);
      return g.qty > 0 ? g.total / g.qty : 0;
    });
    const minSupplierIdx = supplierAvgs.reduce((minIdx, v, idx, arr) => v < arr[minIdx] ? idx : minIdx, 0);
    const barColors = supplierLabels.map((_, i) => i === minSupplierIdx ? '#1D9E75' : '#378ADD');

    const supplierBarEl = document.getElementById('supplierBarChart');
    if (supplierBarEl && window.Chart) {
      if (supplierChartInstance) supplierChartInstance.destroy();
      supplierChartInstance = new Chart(supplierBarEl, {
        type: 'bar',
        data: {
          labels: supplierLabels,
          datasets: [{
            data: supplierAvgs,
            backgroundColor: barColors,
            borderRadius: 6,
            borderSkipped: false
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: ctx => `${ctx.label}: ${fmtCurrency.format(ctx.parsed.y || 0)}`
              }
            }
          },
          scales: {
            y: {
              ticks: { font: { size: 10 }, callback: v => fmtCurrency.format(v) },
              grid: { color: 'rgba(128,128,128,0.1)' }
            },
            x: {
              ticks: { display: false },
              grid: { display: false }
            }
          }
        }
      });
    }

    const supplierLegend = document.getElementById('supplierLegend');
    if (supplierLegend) {
      supplierLegend.innerHTML = supplierLabels.map((label, idx) => {
        const color = barColors[idx];
        const avgLabel = fmtCurrency.format(supplierAvgs[idx] || 0);
        return `<span class="legend-item"><span class="legend-line" style="background:${color}"></span>${label} · ${avgLabel}</span>`;
      }).join('');
    }

    const tbody = document.getElementById('analysisTableBody');
    if (tbody) {
      tbody.innerHTML = '';
      sortedDesc.forEach(x => {
        const deviation = avg ? ((x.PrecoUnitarioNum - avg) / avg) * 100 : 0;
        let pillClass = 'pill-green';
        if (deviation >= 0 && deviation <= 5) pillClass = 'pill-amber';
        if (deviation > 5) pillClass = 'pill-red';
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${x.CodPcNorm}</td>
          <td>${x.DataPcDate ? fmtDate.format(x.DataPcDate) : '-'}</td>
          <td>${x.FornecedorNorm || '-'}</td>
          <td>${x.QuantidadeNum.toLocaleString('pt-BR')}</td>
          <td>${fmtCurrency.format(x.PrecoUnitarioNum)}</td>
          <td><span class="pill ${pillClass}">${deviation >= 0 ? '+' : ''}${deviation.toFixed(1)}%</span></td>
          <td>${fmtCurrency.format(x.ValorTotalItemNum)}</td>
        `;
        tbody.appendChild(tr);
      });
      const rows = Array.from(tbody.querySelectorAll('tr'));
      renderDashboardPagination(rows);
    }
  };

  render(normalized);
})();
