import { Component } from '@angular/core';
import { CreateEscrowComponent } from "./features/escrow/create-escrow/create-escrow.component";
import { PayEscrowComponent } from "./features/checkout/pay-escrow/pay-escrow.component";
import { RouterModule } from "@angular/router";

@Component({
  selector: 'app-root',
  imports: [ RouterModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App  {

}
