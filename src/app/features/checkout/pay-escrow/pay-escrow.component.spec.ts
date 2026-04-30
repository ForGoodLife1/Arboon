import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PayEscrowComponent } from './pay-escrow.component';

describe('PayEscrowComponent', () => {
  let component: PayEscrowComponent;
  let fixture: ComponentFixture<PayEscrowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PayEscrowComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PayEscrowComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
