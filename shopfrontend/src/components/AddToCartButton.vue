<template>
  <div class="flex justify-center sm:justify-start">
    <button
        class="bg-brown-500 text-white py-2 px-4 rounded shadow hover:bg-amber-500 transition duration-300 w-full sm:w-auto flex items-center justify-center"
        @click="addToCart"
    >
      <!-- Cart Icon -->
      <font-awesome-icon
          icon="shopping-cart"
          class="text-white w-5 h-5 mr-2"
      />
    </button>
  </div>
</template>

<script setup>
const props = defineProps({
  product: {
    type: Object,
    required: true, // Product details to be added to the cart
  },
});

const emit = defineEmits(['addToCart']); // Define the event to emit when clicked

const addToCart = () => {
  // Emit the event with the product details
  emit('addToCart', { product: props.product, quantity: 1 });
};
</script>

<style scoped>
button {
  transition: background-color 0.3s ease, transform 0.2s ease;
}

button:hover {
  transform: scale(1.05); /* Subtle zoom-in effect */
}

.flex {
  display: flex;
}
</style>
