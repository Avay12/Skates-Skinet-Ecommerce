import { nanoid } from 'nanoid';

export type CartType = {
  id: string;
  items: CartItem[];
};

export type CartItem = {
  productId: number;
  productName: string;
  price: number;
  quantityt: number;
  pcitureUrl: number;
  brand: string;
  type: string;
};

export class Cart implements CartType {
  id = nanoid();
  items: CartItem[] = [];
}
