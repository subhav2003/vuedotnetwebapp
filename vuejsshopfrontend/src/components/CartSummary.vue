<template>
  <div>
    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center h-40">
      <svg class="animate-spin h-8 w-8 text-brown-700" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle
            class="opacity-25"
            cx="12" cy="12" r="10"
            stroke="currentColor"
            stroke-width="4"
        />
        <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8v8H4z"
        />
      </svg>
    </div>

    <!-- Cart Items -->
    <div
        v-else
        class="space-y-4 overflow-y-auto bg-gray-50 p-4 rounded-md shadow-md"
        style="max-height: 240px;"
    >
      <div
          v-for="item in cartItems"
          :key="item.product.id"
          class="flex items-center justify-between bg-gray-100 p-4 rounded-md shadow"
      >
        <div class="flex items-center space-x-4">
          <img
              :src="item.product.images[0] || 'https://via.placeholder.com/64x64'"
              alt="Product Image"
              class="h-16 w-16 object-cover rounded"
          />
          <div>
            <h4 class="text-md font-semibold text-gray-800">
              {{ item.product.name }}
            </h4>
            <p class="text-sm text-gray-500">
              {{ item.quantity }} × ${{ parseFloat(item.product.price).toFixed(2) }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Summary -->
    <div v-if="!loading" class="mt-6 border-t border-gray-300 pt-4 space-y-2">
      <p class="flex justify-between text-gray-700">
        <span>Subtotal:</span>
        <span>${{ subtotal.toFixed(2) }}</span>
      </p>
      <p class="flex justify-between text-gray-700">
        <span>Shipping:</span>
        <span>${{ shipping.toFixed(2) }}</span>
      </p>
      <p class="flex justify-between text-gray-700">
        <span>Discount:</span>
        <span>-${{ discount.toFixed(2) }}</span>
      </p>
      <p class="flex justify-between font-bold text-lg text-gray-800">
        <span>Total:</span>
        <span>${{ total.toFixed(2) }}</span>
      </p>
    </div>

    <!-- Pay Now -->
    <button
        @click="emitPayNow"
        :disabled="!isPayNowEnabled || loading"
        class="mt-4 bg-orange-500 hover:bg-orange-600 text-white py-2 px-4 rounded-md w-full font-semibold disabled:bg-gray-400 disabled:cursor-not-allowed"
    >
      Pay Now
    </button>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';

const props = defineProps({
  isPayNowEnabled: {
    type: Boolean,
    required: true
  }
});
const emit = defineEmits(['pay-now']);

const apiBaseUrl = process.env.VUE_APP_API_URL;
const cartItems = ref([]);
const loading = ref(true);

// You can adjust these or fetch them from your API if needed
const shipping = ref(5.0);
const discount = ref(5.0);

onMounted(async () => {
  loading.value = true;
  try {
    const token = localStorage.getItem('authToken');
    const { data } = await axios.get(`${apiBaseUrl}/api/v1/cart`, {
      headers: { Authorization: `Bearer ${token}` }
    });

    // data.cart.products is an array of products with a pivot.quantity
    cartItems.value = (data.cart.products || []).map(p => ({
      product: p,
      quantity: p.pivot?.quantity ?? 1
    }));
  } catch (err) {
    console.error('Failed to fetch cart:', err);
  } finally {
    loading.value = false;
  }
});

const subtotal = computed(() =>
    cartItems.value.reduce((sum, item) => {
      return sum + parseFloat(item.product.price) * item.quantity;
    }, 0)
);

const total = computed(() => subtotal.value + shipping.value - discount.value);

function emitPayNow() {
  if (!props.isPayNowEnabled) {
    alert('Please complete all required steps before paying.');
    return;
  }
  emit('pay-now');
}
</script>
