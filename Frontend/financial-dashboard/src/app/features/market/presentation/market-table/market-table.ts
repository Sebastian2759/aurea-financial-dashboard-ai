import { Component, input, output } from '@angular/core';
import { Metric, Quote, exceedsThreshold } from '../../domain/market';
import { money, percent } from '../../../../shared/formatting/format';
@Component({
  selector: 'app-market-table',
  templateUrl: './market-table.html',
  styleUrl: './market-table.scss',
})
export class MarketTable {
  readonly quotes = input.required<Quote[]>();
  readonly currency = input('usd');
  readonly metrics = input.required<Metric[]>();
  readonly threshold = input(5);
  readonly selectAsset = output<string>();
  readonly money = money;
  readonly percent = percent;
  readonly exceeds = exceedsThreshold;
}
