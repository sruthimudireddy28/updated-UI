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

 getAmenities(amenities: string | string[] | null): string[] {
  if (!amenities) {
    return ['WiFi', 'AC', 'TV']; // fallback
  }

  if (Array.isArray(amenities)) {
    return amenities; // already an array
  }

  // if it's a string, split it
  return amenities.split(',').map(s => s.trim()).filter(s => s.length > 0);
}


}
