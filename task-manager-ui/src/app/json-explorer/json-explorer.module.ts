import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// Service
import { JsonExplorerService } from './services/json-explorer.service';

// Components
import { JsonExplorerComponent } from './json-explorer.component';
import { JsonEditorComponent } from './components/editor/editor.component';
import { JsonTreeComponent } from './components/tree/tree.component';
import { JsonCompareComponent } from './components/compare/compare.component';
import { JsonSchemaComponent } from './components/schema/schema.component';
import { JsonConverterComponent } from './components/converter/converter.component';
import { JsonApiClientComponent } from './components/api-client/api-client.component';
import { JsonHistoryComponent } from './components/history/history.component';

@NgModule({
  declarations: [
    JsonExplorerComponent,
    JsonEditorComponent,
    JsonTreeComponent,
    JsonCompareComponent,
    JsonSchemaComponent,
    JsonConverterComponent,
    JsonApiClientComponent,
    JsonHistoryComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ],
  exports: [
    JsonExplorerComponent
  ],
  providers: [
    JsonExplorerService
  ]
})
export class JsonExplorerModule { }
