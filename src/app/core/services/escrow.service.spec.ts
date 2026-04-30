import { TestBed } from '@angular/core/testing';

import { EscrowService } from './escrow.service';

describe('EscrowService', () => {
  let service: EscrowService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EscrowService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
