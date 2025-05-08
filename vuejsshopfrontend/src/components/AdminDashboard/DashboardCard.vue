<template>
  <div
      class="cursor-pointer bg-white p-6 rounded-xl shadow-md hover:shadow-lg transition duration-200 flex flex-col items-center text-center"
      @click="$emit('click')"
  >
    <div class="text-4xl mb-3">{{ icon }}</div>
    <h3 class="text-lg font-semibold">{{ title }}</h3>
  </div>
</template>

<script setup>
defineProps({
  title: String,
  icon: String,
})
</script>