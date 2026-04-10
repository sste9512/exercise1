import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AstronautService } from '../../core/services/astronaut.service';
import { GetAstronautDutiesResponse, AstronautDuty, PersonAstronaut } from '../../core/models/astronaut.model';

type LoadingState = 'idle' | 'loading' | 'success' | 'error';

@Component({
  selector: 'app-astronaut-duties',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './astronaut-duties.component.html',
  styleUrls: ['./astronaut-duties.component.scss']
})
export class AstronautDutiesComponent {
  searchName = signal<string>('');
  loadingState = signal<LoadingState>('idle');
  loadingProgress = signal<number>(0);
  errorMessage = signal<string>('');
  
  person = signal<PersonAstronaut | null>(null);
  duties = signal<AstronautDuty[]>([]);

  isIdle = computed(() => this.loadingState() === 'idle');
  isLoading = computed(() => this.loadingState() === 'loading');
  isSuccess = computed(() => this.loadingState() === 'success');
  isError = computed(() => this.loadingState() === 'error');

  hasResults = computed(() => this.person() !== null);
  totalDuties = computed(() => this.duties().length);
  
  careerDuration = computed(() => {
    const p = this.person();
    if (!p?.careerStartDate) return null;
    
    const start = new Date(p.careerStartDate);
    const end = p.careerEndDate ? new Date(p.careerEndDate) : new Date();
    const years = Math.floor((end.getTime() - start.getTime()) / (365.25 * 24 * 60 * 60 * 1000));
    const months = Math.floor(((end.getTime() - start.getTime()) % (365.25 * 24 * 60 * 60 * 1000)) / (30.44 * 24 * 60 * 60 * 1000));
    
    if (years > 0) {
      return `${years} year${years !== 1 ? 's' : ''}, ${months} month${months !== 1 ? 's' : ''}`;
    }
    return `${months} month${months !== 1 ? 's' : ''}`;
  });

  isRetired = computed(() => {
    const p = this.person();
    return p?.careerEndDate !== null && p?.careerEndDate !== undefined;
  });

  constructor(private astronautService: AstronautService) {}

  search(): void {
    const name = this.searchName().trim();
    if (!name) {
      this.errorMessage.set('Please enter a name to search');
      this.loadingState.set('error');
      return;
    }

    this.loadingState.set('loading');
    this.loadingProgress.set(0);
    this.errorMessage.set('');
    this.person.set(null);
    this.duties.set([]);

    const progressInterval = setInterval(() => {
      const current = this.loadingProgress();
      if (current < 90) {
        this.loadingProgress.set(current + Math.random() * 15);
      }
    }, 200);

    this.astronautService.getAstronautDutiesByName(name).subscribe({
      next: (response: GetAstronautDutiesResponse) => {
        clearInterval(progressInterval);
        this.loadingProgress.set(100);
        
        setTimeout(() => {
          this.person.set(response.person);
          this.duties.set(response.astronautDuties || []);
          this.loadingState.set('success');
        }, 300);
      },
      error: (err) => {
        clearInterval(progressInterval);
        this.loadingProgress.set(0);
        this.errorMessage.set(err.error?.message || err.message || 'Failed to retrieve astronaut duties');
        this.loadingState.set('error');
      }
    });
  }

  formatDate(dateString: string | null): string {
    if (!dateString) return 'Present';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  getDutyDuration(duty: AstronautDuty): string {
    const start = new Date(duty.dutyStartDate);
    const end = duty.dutyEndDate ? new Date(duty.dutyEndDate) : new Date();
    const days = Math.floor((end.getTime() - start.getTime()) / (24 * 60 * 60 * 1000));
    
    if (days < 30) return `${days} days`;
    if (days < 365) return `${Math.floor(days / 30)} months`;
    return `${Math.floor(days / 365)} years`;
  }

  isCurrentDuty(duty: AstronautDuty): boolean {
    return duty.dutyEndDate === null;
  }

  reset(): void {
    this.searchName.set('');
    this.loadingState.set('idle');
    this.loadingProgress.set(0);
    this.errorMessage.set('');
    this.person.set(null);
    this.duties.set([]);
  }
}
