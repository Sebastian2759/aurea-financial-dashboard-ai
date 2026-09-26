import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MarketFacade } from '../../application/market.facade';
import { ASSETS } from '../../domain/market';
import { money, percent } from '../../../../shared/formatting/format';
import { PriceChart } from '../price-chart/price-chart';
import { MarketTable } from '../market-table/market-table';
import { MetricSelector } from '../metric-selector/metric-selector';
import { CurrencySelector } from '../currency-selector/currency-selector';
import { ConnectionStatus } from '../connection-status/connection-status';
@Component({
  selector: 'app-market-page',
  imports: [DatePipe, PriceChart, MarketTable, MetricSelector, CurrencySelector, ConnectionStatus],
  templateUrl: './market-page.html',
  styleUrl: './market-page.scss',
})
export class MarketPage {
  readonly vm = inject(MarketFacade);
  readonly assets = ASSETS;
  readonly money = money;
  readonly percent = percent;
}
