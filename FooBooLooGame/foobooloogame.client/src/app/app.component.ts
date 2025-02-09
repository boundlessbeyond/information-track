import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  formGroup: FormGroup<UserGame>;

  constructor(private http: HttpClient, public formBuilder: FormBuilder) {
    this.formGroup = this.formBuilder.group<UserGame>({
      name: this.formBuilder.control({ value: '', disabled: false }),
      minimum: this.formBuilder.control({ value: '', disabled: false }),
      maximum: this.formBuilder.control(''),
      timeLimit: this.formBuilder.control(''),
      gameRules: new FormArray<FormGroup<GameRule>>([]),
    }) as FormGroup;
  }

  ngOnInit() {}

  submit(): void {
    for (
      let index = 0;
      index < this.formGroup.controls.gameRules.length;
      index++
    ) {
      const element = this.formGroup.controls.gameRules.controls[index];
      var test = element.controls.divisor;
      var test1 = element.controls.word;
    }

    const formValue = this.formGroup.getRawValue();

    const dto: CreateGameDto = {
      GameName: formValue.name!,
      MinNumber: Number(formValue.minimum!),
      MaxNumber: Number(formValue.maximum!),
      TimeLimit: Number(formValue.timeLimit!),
      NumbersToWords: formValue.gameRules.map(g => {
        return <NumberToWordDto> {
          Number: Number(g.divisor!),
          Word: g.word!
        }
      })
    };

    this.http.post<CreateGameDto>('/Game/create-game', dto).subscribe({
      next: (result) => alert('success'),
      error: (error) => alert('fail')
    });
  }
  // getForecasts() {
  //   this.http.get<WeatherForecast[]>('/weatherforecast').subscribe(
  //     (result) => {
  //       this.forecasts = result;
  //     },
  //     (error) => {
  //       console.error(error);
  //     }
  //   );
  // }

  addAlias() {
    this.formGroup.controls.gameRules.push(
      this.formBuilder.group<GameRule>({
        divisor: this.formBuilder.control(''),
        word: this.formBuilder.control(''),
      })
    );
  }

  title = 'foobooloogame.client';
}

interface UserGame {
  name?: FormControl<string | null>;
  minimum?: FormControl<string | null>;
  maximum?: FormControl<string | null>;
  timeLimit?: FormControl<string | null>;
  gameRules: FormArray<FormGroup<GameRule>>;
}

interface GameRule {
  divisor?: FormControl<string | null>;
  word?: FormControl<string | null>;
}

interface CreateGameDto {
  GameName: string;
  MinNumber: number;
  MaxNumber: number;
  TimeLimit: number;
  NumbersToWords: NumberToWordDto[]
}

interface NumberToWordDto {
  Number: number;
  Word: string;
}
