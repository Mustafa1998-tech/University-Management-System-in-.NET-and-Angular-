import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';

interface PageDesignData {
  title?: string;
  description?: string;
  cards?: string[];
  formFields?: string[];
  filters?: string[];
  tableHeaders?: string[];
  actions?: string[];
  sections?: string[];
}

const DEFAULT_TITLE = 'Page';
const DEFAULT_DESCRIPTION = 'This page scaffold is ready for implementation.';

@Component({
  selector: 'app-page-placeholder',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page">
      <header class="page-header">
        <h1>{{ title }}</h1>
        <p>{{ description }}</p>
      </header>

      <div class="content-grid">
        <article *ngIf="cards.length" class="block">
          <h2>Cards</h2>
          <div class="chips">
            <span *ngFor="let card of cards" class="chip">{{ card }}</span>
          </div>
        </article>

        <article *ngIf="formFields.length" class="block">
          <h2>Form Fields</h2>
          <div class="form-preview">
            <label *ngFor="let field of formFields" class="form-row">
              <span>{{ field }}</span>
              <input type="text" [placeholder]="field" disabled />
            </label>
          </div>
        </article>

        <article *ngIf="filters.length" class="block">
          <h2>Filters / Search</h2>
          <div class="chips">
            <span *ngFor="let filter of filters" class="chip alt">{{ filter }}</span>
          </div>
        </article>

        <article *ngIf="sections.length" class="block">
          <h2>Sections</h2>
          <ul>
            <li *ngFor="let section of sections">{{ section }}</li>
          </ul>
        </article>

        <article *ngIf="tableHeaders.length" class="block table-wrap">
          <h2>Data Table</h2>
          <div class="table">
            <div class="row header">
              <span *ngFor="let head of tableHeaders">{{ head }}</span>
            </div>
            <div class="row">
              <span *ngFor="let head of tableHeaders">...</span>
            </div>
          </div>
        </article>

        <article *ngIf="actions.length" class="block">
          <h2>Actions</h2>
          <div class="actions">
            <button *ngFor="let action of actions" type="button">{{ action }}</button>
          </div>
        </article>
      </div>
    </section>
  `,
  styles: [`
    .page {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: 12px;
      padding: 20px;
      box-shadow: var(--shadow-sm);
    }

    h1 {
      margin: 0 0 10px;
      font-size: 1.5rem;
      color: var(--color-primary-700);
    }

    p {
      margin: 0;
      color: var(--color-text-secondary);
      line-height: 1.5;
    }

    .content-grid {
      margin-top: 18px;
      display: grid;
      gap: 12px;
    }

    .block {
      border: 1px dashed var(--color-primary-200);
      border-radius: 10px;
      padding: 14px;
      background: #f8f9fc;
    }

    .block h2 {
      margin: 0 0 10px;
      font-size: 1rem;
      color: var(--color-primary-700);
    }

    .chips {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .chip {
      padding: 8px 10px;
      border-radius: 999px;
      background: var(--color-primary-100);
      color: var(--color-primary-900);
      font-size: 0.85rem;
    }

    .chip.alt {
      background: var(--color-secondary-100);
      color: var(--color-secondary-600);
    }

    ul {
      margin: 0;
      padding-left: 20px;
      color: var(--color-text-secondary);
      display: grid;
      gap: 6px;
    }

    .table {
      border: 1px solid var(--color-border);
      border-radius: 8px;
      overflow: hidden;
      background: var(--color-surface);
    }

    .form-preview {
      display: grid;
      gap: 8px;
    }

    .form-row {
      display: grid;
      gap: 6px;
      color: var(--color-text-primary);
      font-size: 0.86rem;
    }

    .form-row input {
      border: 1px solid var(--color-border);
      border-radius: 8px;
      padding: 8px 10px;
      background: var(--color-surface);
      color: var(--color-text-secondary);
      font-size: 0.84rem;
    }

    .row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
      gap: 8px;
      padding: 10px;
      border-top: 1px solid #edf1f7;
      font-size: 0.87rem;
      color: var(--color-text-secondary);
    }

    .row.header {
      border-top: none;
      background: var(--color-primary-50);
      font-weight: 600;
      color: var(--color-primary-800);
    }

    .actions {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    button {
      border: none;
      background: var(--color-primary-600);
      color: #fff;
      padding: 8px 12px;
      border-radius: 8px;
      font-size: 0.85rem;
      cursor: pointer;
    }

    button:hover {
      background: var(--color-primary-700);
    }
  `]
})
export class PagePlaceholderComponent {
  private readonly destroyRef = inject(DestroyRef);

  title = DEFAULT_TITLE;
  description = DEFAULT_DESCRIPTION;
  cards: string[] = [];
  formFields: string[] = [];
  filters: string[] = [];
  tableHeaders: string[] = [];
  actions: string[] = [];
  sections: string[] = [];

  constructor(private route: ActivatedRoute) {
    this.route.data
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((data) => this.applyData(data as PageDesignData));
  }

  private applyData(pageData: PageDesignData): void {
    this.title = pageData.title || DEFAULT_TITLE;
    this.description = pageData.description || DEFAULT_DESCRIPTION;
    this.cards = pageData.cards || [];
    this.formFields = pageData.formFields || [];
    this.filters = pageData.filters || [];
    this.tableHeaders = pageData.tableHeaders || [];
    this.actions = pageData.actions || [];
    this.sections = pageData.sections || [];
  }
}
