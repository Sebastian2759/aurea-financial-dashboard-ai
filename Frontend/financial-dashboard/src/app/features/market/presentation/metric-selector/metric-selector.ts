import { Component, input, output } from '@angular/core';
import { METRICS, Metric } from '../../domain/market';
@Component({
  selector: 'app-metric-selector',
  templateUrl: './metric-selector.html',
  styleUrl: './metric-selector.scss',
})
export class MetricSelector {
  readonly selected = input.required<Metric[]>();
  readonly toggle = output<Metric>();
  readonly metrics = METRICS;
}
