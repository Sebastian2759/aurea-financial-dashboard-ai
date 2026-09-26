import { Component, input, output } from '@angular/core';
import { Owner } from '../../watchlists/domain/watchlist';
@Component({ selector: 'app-owner-selector', templateUrl: './owner-selector.html' })
export class OwnerSelector {
  readonly owners = input.required<Owner[]>();
  readonly owner = input.required<string>();
  readonly busy = input(false);
  readonly selected = output<string>();
}
