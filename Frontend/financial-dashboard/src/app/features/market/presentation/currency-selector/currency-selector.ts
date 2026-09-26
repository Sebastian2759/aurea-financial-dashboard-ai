import { Component, input, output } from '@angular/core';
import { Currency } from '../../domain/market';
@Component({ selector: 'app-currency-selector', templateUrl: './currency-selector.html' })
export class CurrencySelector {
  readonly currency = input.required<Currency>();
  readonly selected = output<Currency>();
}
