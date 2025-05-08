<template>
  <div class="mt-6">
    <h3 class="text-lg font-semibold mb-2">Payment Method</h3>
    <div class="space-y-4">
      <div v-for="method in paymentMethods" :key="method.id">
        <input
            type="radio"
            :id="method.id"
            name="payment"
            class="mr-2"
            :value="method.id"
            @change="selectPayment(method.id)"
        >
        <label :for="method.id" class="cursor-pointer">{{ method.name }}</label>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, defineEmits } from 'vue';

// Define the event emitter
const emit = defineEmits(['payment-selected']);

// Define the payment methods in an array
const paymentMethods = ref([
  {
    id: 'esewa',
    name: 'eSewa',
  },

  // Add additional payment methods here later
]);

// Store the selected payment method
const selectedPayment = ref('');

// Emit the selected payment method
const selectPayment = (paymentMethodId) => {
  selectedPayment.value = paymentMethodId;
  emit('payment-selected', selectedPayment.value);
};
</script>
