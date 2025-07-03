import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { NgChartsModule } from 'ng2-charts';
import { ChartData, ChartOptions, Chart } from 'chart.js';
import ChartDataLabels from 'chartjs-plugin-datalabels';
import { TimeEntryService } from '../../services/time-entry.service';
import { Entry } from '../../models/entry.model';


interface EmployeeTotalHours {
  name: string;
  hours: number;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, HttpClientModule, NgChartsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {

  public processedEntries: EmployeeTotalHours[] = [];

  constructor(private http: HttpClient, private timeEntryService: TimeEntryService){
    Chart.register(ChartDataLabels);
  }

  ngOnInit(): void{
    this.getData();
  }

  public getData(){
      this.timeEntryService.getEntries().subscribe((entries) => {      
      const totalHoursMap: {[emp: string]: EmployeeTotalHours } = {};
      
      for(const entry of entries) {

        if (entry.DeletedOn) continue

        if (!entry.EmployeeName) {
            console.warn('ID:', entry.Id, 'This entry has no name:');
            continue; 
          }

        if(!totalHoursMap[entry.EmployeeName]){
          totalHoursMap[entry.EmployeeName] = {
            name: entry.EmployeeName,
            hours: 0
          }
        }
          totalHoursMap[entry.EmployeeName].hours += this.calculateHours(entry.Id, entry.StarTimeUtc, entry.EndTimeUtc);
      }
      this.processedEntries = Object.values(totalHoursMap).sort((a, b) => b.hours - a.hours);
      this.updatePieChart();
    })
  }

  private calculateHours(id: string, startTime: string, endTime: string):number{

    const start = new Date(startTime);
    const end = new Date(endTime);
    
    if (isNaN(start.getTime()) || isNaN(end.getTime())) {
      console.warn('Entry ID:', id,'Invalid date input:', startTime, endTime);
      return 0;
    }

    if (end < start) {
      console.warn('Entry ID:', id,'End time is before start time:', endTime, '<', startTime);
      return 0;
    } 
    
    const durationMs = end.getTime() - start.getTime();
    //const durationMs = Math.abs(end.getTime() - start.getTime()); // Pretpostavka: zamenjeno start i end vreme (bez provere na liniji 79)
    const durationHours = durationMs / (1000 * 60 * 60);
    return durationHours;
  }

  public pieChartData: ChartData<'pie', number[], string> = {
    labels:[],
    datasets:[{
      data:[],
      backgroundColor:[],
    }]
  };

  
public pieChartOptions: ChartOptions<'pie'> = {
  responsive: true,
  plugins: {
    legend: {
      position: 'bottom',
    },
    tooltip: {
      enabled: true,
    },
    datalabels: {
      formatter: (value: number, ctx) => {
        const data = ctx.chart.data.datasets[0].data as number[];
        const total = data.reduce((acc, curr)=> acc + curr, 0);
        const percentage = ((value / total) * 100).toFixed(2) + '%';
        return percentage;
      },
      color: '#fff',
      font: {
        weight: 'bold',
        size: 13,
      },
      align: 'end',
      offset: 20
    },
  },
};
  
  private updatePieChart() {
    this.pieChartData.labels = this.processedEntries.map(e => e.name);
    this.pieChartData.datasets[0].data = this.processedEntries.map(e => e.hours);
    this.pieChartData.datasets[0].backgroundColor = this.generateColors(this.processedEntries.length);
  }
  private generateColors(count: number): string[] {
    const baseColors = [
      '#FF6384', 
      '#36A2EB', 
      '#FFCE56', 
      '#4BC0C0', 
      '#9966FF',
      '#FF9F40', 
      '#66FF66', 
      '#FF66B2', 
      '#66B2FF', 
      '#FFB266',
      '#B266FF',
      '#00CC99' 
    ];
    const colors = [];
    for (let i = 0; i < count; i++) {
      colors.push(baseColors[i % baseColors.length]);
    }
    return colors;
  }

}
