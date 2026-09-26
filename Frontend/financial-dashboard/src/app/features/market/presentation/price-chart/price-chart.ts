import { Component, ElementRef, effect, input, OnDestroy, viewChild } from '@angular/core';
import { Chart, registerables } from 'chart.js';
import { History } from '../../domain/market';
import { money } from '../../../../shared/formatting/format';
Chart.register(...registerables);
@Component({
  selector: 'app-price-chart',
  templateUrl: './price-chart.html',
  styleUrl: './price-chart.scss',
})
export class PriceChart implements OnDestroy {
  readonly history = input.required<History>();
  private canvas = viewChild<ElementRef<HTMLCanvasElement>>('canvas');
  private chart?: Chart;
  constructor() {
    effect(() => {
      const canvas = this.canvas(),
        history = this.history();
      if (!canvas) return;
      this.chart?.destroy();
      this.chart = new Chart(canvas.nativeElement, {
        type: 'line',
        data: {
          labels: history.points.map((p) =>
            new Date(p.time).toLocaleString('es-ES', {
              day: '2-digit',
              month: 'short',
              hour: '2-digit',
              minute: '2-digit',
            }),
          ),
          datasets: [
            {
              data: history.points.map((p) => p.price),
              borderColor: '#206b58',
              backgroundColor: 'rgba(32,107,88,.08)',
              fill: true,
              tension: 0.2,
              borderWidth: 2,
              pointRadius: 0,
              pointHoverRadius: 5,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          animation: false,
          interaction: { intersect: false, mode: 'index' },
          plugins: {
            legend: { display: false },
            tooltip: { callbacks: { label: (item) => money(Number(item.raw), history.currency) } },
          },
          scales: {
            x: {
              grid: { display: false },
              ticks: { maxTicksLimit: 6, maxRotation: 0, color: '#7c828a' },
            },
            y: {
              position: 'right',
              grid: { color: '#edf0ee' },
              ticks: {
                maxTicksLimit: 5,
                color: '#7c828a',
                callback: (value) => money(Number(value), history.currency, true),
              },
            },
          },
        },
      });
    });
  }
  ngOnDestroy() {
    this.chart?.destroy();
  }
}
