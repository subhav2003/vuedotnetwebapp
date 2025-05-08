<template>
  <div
      class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-4 rounded shadow transition"
  >
    <!-- Image -->
    <div class="w-full sm:w-24 h-24 flex-shrink-0">
      <img
          :src="product.images?.[0] || 'https://via.placeholder.com/150'"
          alt="Product"
          class="w-full h-full object-cover rounded"
      />
    </div>

    <!-- Details -->
    <div class="flex-1 text-center sm:text-left">
      <h2 class="text-lg font-semibold text-gray-800">{{ product.name }}</h2>
      <p class="text-sm text-gray-600">Price: ${{ formattedPrice }}</p>
    </div>

    <!-- Quantity & Remove -->
    <div class="flex flex-col sm:items-end items-center gap-2">
      <div class="flex items-center gap-2">
        <button
            @click="decreaseQuantity"
            :disabled="quantity === 1"
            class="px-2 py-1 bg-gray-200 hover:bg-gray-300 rounded disabled:opacity-50"
        >
          -
        </button>
        <span class="min-w-[1.5rem] text-center">{{ quantity }}</span>
        <button
            @click="increaseQuantity"
            class="px-2 py-1 bg-gray-200 hover:bg-gray-300 rounded"
        >
          +
        </button>
      </div>

      <button
          @click="removeItem"
          class="mt-1 bg-red-500 text-white text-sm px-3 py-1 rounded hover:bg-red-600"
      >
        Remove
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';

const props = defineProps({
  product: { type: Object, required: true },
  initialQuantity: { type: Number, required: true },
});

const emit = defineEmits(['updateQuantity', 'removeItem']);
const quantity = ref(props.initialQuantity);

// Sync with parent updates
watch(() => props.initialQuantity, (newVal) => {
  quantity.value = newVal;
});

const formattedPrice = computed(() => Number(props.product.price).toFixed(2));

const increaseQuantity = () => {
  quantity.value++;
  emit('updateQuantity', { productId: props.product.id, quantity: quantity.value });
};

const decreaseQuantity = () => {
  if (quantity.value > 1) {
    quantity.value--;
    emit('updateQuantity', { productId: props.product.id, quantity: quantity.value });
  }
};

const removeItem = () => {
  emit('removeItem', props.product.id);
};
</script>
