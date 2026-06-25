import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-room-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './room-card.component.html',
  styleUrl: './room-card.component.css'
})
export class RoomCardComponent {
  room = input.required<any>();
  canManage = input<boolean>(false);
  book = output<any>();
  edit = output<any>();
  delete = output<any>();

  onBookClick() {
    this.book.emit(this.room());
  }

  onEditClick() {
    this.edit.emit(this.room());
  }

  onDeleteClick() {
    this.delete.emit(this.room());
  }

 getAmenities(amenities: any[] | string | null): string[] {
  if (!amenities) {
    return ['WiFi', 'AC', 'TV']; // fallback
  }

  if (Array.isArray(amenities)) {
    return amenities.map(a => {
      if (typeof a === 'object' && a !== null) {
        return a.name || '';
      }
      return String(a);
    }).filter(s => s.length > 0);
  }

  // if it's a string, split it
  return amenities.split(',').map(s => s.trim()).filter(s => s.length > 0);
}


}
