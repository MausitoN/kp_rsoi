import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  MatButtonModule, MatCardModule, MatDialogModule, MatInputModule, MatTableModule,
  MatToolbarModule, MatMenuModule, MatIconModule, MatProgressSpinnerModule,
  MatGridListModule, MatListModule, MatCheckboxModule, MatStepperModule,
  MatChipsModule,
  MatPaginatorModule,
  MatSortModule
} from '@angular/material';
import { MatSliderModule } from '@angular/material/slider';

/**
 * Массив импортируемых и экспортируемых модулей MaterialDesign
 */
const materialModules = [
  CommonModule,
  MatToolbarModule,
  MatButtonModule,
  MatCardModule,
  MatInputModule,
  MatDialogModule,
  MatTableModule,
  MatMenuModule,
  MatIconModule,
  MatProgressSpinnerModule,
 
  MatGridListModule,
  MatListModule,
  MatCheckboxModule,
  MatStepperModule,
  MatChipsModule,
  MatPaginatorModule,
  MatSortModule,
  MatSliderModule
];

@NgModule({
  imports: materialModules,
  exports: materialModules
})
export class MaterialModule { }
