import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DisputeCenterComponent } from './dispute-center.component';

describe('DisputeCenterComponent', () => {
  let component: DisputeCenterComponent;
  let fixture: ComponentFixture<DisputeCenterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DisputeCenterComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DisputeCenterComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
